#include "pch.h"
#include <wrl.h>
#include <Robuffer.h>
#include <algorithm>
#include <MemoryBuffer.h>
#include <comdef.h>
#include <string>

#include "MainParagraphFinder.h"
#include <opencv2/imgproc/imgproc.hpp>
#include <gsl.h>

using namespace TLDR;
using namespace Platform;
using namespace Windows::Foundation;
using namespace Windows::Graphics::Imaging;
using namespace Microsoft::WRL;
using namespace Windows::Storage::Streams;

Windows::Graphics::Imaging::SoftwareBitmap^ TLDR::FindMainParagraph::ExtractMainParagraph(Windows::Graphics::Imaging::SoftwareBitmap ^ bitmap)
{
	auto src = getMat(bitmap);
	auto currentType = src.type();
	cv::Mat img;
	cv::resize(src, img, cv::Size(src.size().width / 5, src.size().height / 5), 0, 0, CV_INTER_AREA);
	cv::cvtColor(img, img, CV_RGBA2GRAY);
	currentType = img.type();

	auto kernel = cv::Mat::ones(10, 10, CV_8U);

	cv::erode(img, img, kernel, cv::Point(-1, -1), 2);
	cv::threshold(img, img, 100, 255, cv::THRESH_BINARY);
	currentType = img.type();
	
	std::vector<cv::Mat> contours;
	cv::findContours(img, contours, CV_RETR_LIST, CV_CHAIN_APPROX_NONE);
	std::nth_element(contours.begin(), contours.begin() + 1, contours.end(), [](const auto& c1, const auto& c2) {return c1.cols * c1.rows > c2.cols * c2.rows; });
	cv::drawContours(img, contours, 1, cv::Scalar(255.0), CV_FILLED);
	currentType = img.type();


	cv::resize(img, img, src.size());
	cv::Mat resultMat;
	src.copyTo(resultMat, img);
	currentType = resultMat.type();
	auto result = getBitmap(resultMat);

	return result;
}

cv::Mat TLDR::FindMainParagraph::getErodedImage(const cv::Mat& img, int kernelSize, int lowThreshold, int highThreshold, int erodeIterations)
{
	cv::Mat result(img.size(), CV_8UC4);
	img.copyTo(result);
	cv::cvtColor(result, result, CV_RGBA2GRAY);
	auto kernel = cv::Mat::ones(kernelSize, kernelSize, CV_8U);

	cv::erode(result, result, kernel, cv::Point(-1, -1), erodeIterations);
	cv::threshold(result, result, lowThreshold, highThreshold, cv::THRESH_BINARY);

	return result;
}

std::vector<cv::Mat> TLDR::FindMainParagraph::getContours(const cv::Mat& img)
{
	std::vector<cv::Mat> contours;
	cv::findContours(img, contours, CV_RETR_LIST, CV_CHAIN_APPROX_NONE);
	
	return contours;
}

cv::Mat TLDR::FindMainParagraph::getContourMask(const std::vector<cv::Mat>& contours, int index, const cv::Size& size)
{
	cv::Mat result(size, CV_8U);
	result = cv::Scalar(0.0);
	cv::drawContours(result, contours, index, cv::Scalar(225.0), CV_FILLED);
	return result;
}

cv::Mat TLDR::FindMainParagraph::applyMask(const cv::Mat& img, const cv::Mat& mask)
{
	cv::Mat result;
	img.copyTo(result, mask);
	return result;
}

TLDR::FindMainParagraph::FindMainParagraph(SoftwareBitmap^ bitmap) : m_bitmap(bitmap)
{
	auto kernelSize = 30;
	auto erodeIterations = 2;
	auto lowThreshold = 100;
	auto highThreshold = 255;

	reCalculate(kernelSize, erodeIterations, lowThreshold, highThreshold);
}

SoftwareBitmap^ TLDR::FindMainParagraph::FinalImage()
{
	return getBitmap(m_final);
}

SoftwareBitmap^ TLDR::FindMainParagraph::Recalculate(int kernelSize, int erodeIteration, int lowThreshold, int highThreshold)
{
	reCalculate(kernelSize, erodeIteration, lowThreshold, highThreshold);
	return getBitmap(m_final);
}

void TLDR::FindMainParagraph::reCalculate(int kernelSize, int erodeIteration, int lowThreshold, int highThreshold)
{
	auto src = getMat(m_bitmap);
	m_eroded = getErodedImage(src, kernelSize, lowThreshold, highThreshold, erodeIteration);

	m_contours = getContours(m_eroded);

	std::nth_element(m_contours.begin(), m_contours.begin() + 1, m_contours.end(), [](const auto& c1, const auto& c2) {return c1.cols * c1.rows > c2.cols * c2.rows; });
	auto contour = getContourMask(m_contours, 1, m_eroded.size());

	m_final = applyMask(src, contour);

	m_eroded = applyMask(src, m_eroded);
}

SoftwareBitmap^ TLDR::FindMainParagraph::ErodedImage()
{
	return getBitmap(m_eroded);
}

cv::Mat TLDR::FindMainParagraph::getAllContourImage(const std::vector<cv::Mat>& contours, const cv::Size& size)
{
	cv::Mat result(size, CV_8U);
	result = cv::Scalar(0.0);
	for (auto i = 0u; i < contours.size(); ++i)
	{
		cv::drawContours(result, contours, i, cv::Scalar(225.0), 2);
	}

	return result;
}

SoftwareBitmap^ TLDR::FindMainParagraph::AllContoursImage()
{
	auto allContours = getAllContourImage(m_contours, m_eroded.size());

	cv::Mat result;
	cv::cvtColor(allContours, result, CV_GRAY2RGBA);

	return getBitmap(allContours);
}

using namespace std::string_literals;

void throwIfError(HRESULT hr, const std::string& errorString)
{
	if (hr != S_OK)
	{
		_com_error err(hr, nullptr);
		std::wstring e(err.ErrorMessage());

		throw std::runtime_error(errorString + std::string(e.begin(), e.end()));
	}
}

cv::Mat TLDR::FindMainParagraph::getMat(Windows::Graphics::Imaging::SoftwareBitmap^ bitmap)
{
	auto buffer = bitmap->LockBuffer(BitmapBufferAccessMode::ReadWrite);
	auto reference = buffer->CreateReference();

	ComPtr<IMemoryBufferByteAccess> bufferByteAccess;
	ComPtr<IInspectable> pBuffer(reinterpret_cast<IInspectable*>(reference));
	auto hr = pBuffer.As(&bufferByteAccess);
	throwIfError(hr, "Cannot get byteBufferAccess"s);

	unsigned char* data = nullptr;
	unsigned int capacity = 0;
	hr = bufferByteAccess->GetBuffer(&data, &capacity);
	throwIfError(hr, "Cannot get buffer"s);
	
	cv::Mat img(bitmap->PixelHeight, bitmap->PixelWidth, CV_8UC4);
	std::copy(data, data + (4 * bitmap->PixelHeight * bitmap->PixelWidth), img.data);

	return img;
}

Windows::Graphics::Imaging::SoftwareBitmap ^ TLDR::FindMainParagraph::getBitmap(const cv::Mat & mat)
{
	auto bitmap = ref new SoftwareBitmap(BitmapPixelFormat::Bgra8, mat.cols, mat.rows, BitmapAlphaMode::Ignore);
	auto buffer = bitmap->LockBuffer(BitmapBufferAccessMode::ReadWrite);
	auto reference = buffer->CreateReference();

	ComPtr<IMemoryBufferByteAccess> bufferByteAccess;
	ComPtr<IInspectable> pBuffer(reinterpret_cast<IInspectable*>(reference));
	auto hresult = pBuffer.As(&bufferByteAccess);
	if (hresult != S_OK)
	{
		throw std::runtime_error("cannot get byteBufferAccess");
	}
	unsigned char* data = nullptr;

	unsigned int capacity = 0;
	bufferByteAccess->GetBuffer(&data, &capacity);

	std::copy(mat.data, mat.data + (mat.total() * mat.elemSize()), data);

	return bitmap;
}

