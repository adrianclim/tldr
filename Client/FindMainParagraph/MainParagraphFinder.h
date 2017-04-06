#pragma once
#include <opencv2/core/core.hpp>

namespace TLDR
{
    public ref class FindMainParagraph sealed
    {
    public:
		static Windows::Graphics::Imaging::SoftwareBitmap^ ExtractMainParagraph(Windows::Graphics::Imaging::SoftwareBitmap^ bitmap);
	private:
		static cv::Mat getMat(Windows::Graphics::Imaging::SoftwareBitmap ^ bitmap);
		static Windows::Graphics::Imaging::SoftwareBitmap^ getBitmap(const cv::Mat& mat);
    };
}
