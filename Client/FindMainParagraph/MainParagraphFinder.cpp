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
	cv::Mat img;
	cv::resize(src, img, cv::Size(src.size().width / 5, src.size().height / 5), 0, 0, CV_INTER_AREA);
	cv::cvtColor(img, img, CV_RGBA2GRAY);

	auto kernel = cv::Mat::ones(5, 5, CV_8U);

	cv::erode(img, img, kernel, cv::Point(-1, -1), 5);
	/*cv::threshold(img, img, 100, 255, cv::THRESH_BINARY);
	
	std::vector<cv::Mat> contours;
	cv::findContours(img, contours, CV_RETR_LIST, CV_CHAIN_APPROX_NONE);
	std::nth_element(contours.begin(), contours.begin() + 1, contours.end(), [](const auto& c1, const auto& c2) {return c1.cols * c1.rows > c2.cols * c2.rows; });
	cv::drawContours(img, contours, 1, cv::Scalar(255.0), CV_FILLED);
*/
	cv::resize(img, img, src.size());
	cv::Mat resultMat;
	src.copyTo(resultMat, img);
	auto result = getBitmap(resultMat);
	return result;
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

