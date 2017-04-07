#pragma once
#include <opencv2/core/core.hpp>
#include <tuple>

namespace TLDR
{
    public ref class FindMainParagraph sealed
    {
    public:
		static Windows::Graphics::Imaging::SoftwareBitmap^ ExtractMainParagraph(Windows::Graphics::Imaging::SoftwareBitmap^ bitmap);
	
		FindMainParagraph(Windows::Graphics::Imaging::SoftwareBitmap^ bitmap);

		Windows::Graphics::Imaging::SoftwareBitmap^ ErodedImage();
		Windows::Graphics::Imaging::SoftwareBitmap^ AllContoursImage();
		Windows::Graphics::Imaging::SoftwareBitmap^ Recalculate(int kernelSize, int erodeIteration, int lowThreshold, int highThreshold);
		Windows::Graphics::Imaging::SoftwareBitmap^ FinalImage();
	
	private:
		static cv::Mat getMat(Windows::Graphics::Imaging::SoftwareBitmap ^ bitmap);
		static Windows::Graphics::Imaging::SoftwareBitmap^ getBitmap(const cv::Mat& mat);
		static cv::Mat getErodedImage(const cv::Mat& img, int kernelSize, int lowThreshold, int highThreshold, int erodeIterations);
		static std::vector<cv::Mat> getContours(const cv::Mat& img);
		static cv::Mat getContourMask(const std::vector<cv::Mat>& contours, int index, const cv::Size& size);
		static cv::Mat applyMask(const cv::Mat& img, const cv::Mat& mask);
		static cv::Mat getAllContourImage(const std::vector<cv::Mat>& contours, const cv::Size& size);
		
		void reCalculate(int kernelSize, int erodeIteration, int lowThreshold, int highThreshold);
		
		Windows::Graphics::Imaging::SoftwareBitmap^ m_bitmap;
		cv::Mat m_final;
		cv::Mat m_eroded;
		std::vector<cv::Mat> m_contours;
	};
}
