from rest_framework.views import APIView
from summarizer.analysis import extract_summary
from rest_framework.response import Response
from rest_framework.permissions import AllowAny


# Create your views here.


class AnalysisRunView(APIView):
    permission_classes = [AllowAny]
    """
    Run an Analysis
    """
    def post(self, request):
        summary = extract_summary("", request.data['content'])
        return Response({'summary': summary})
