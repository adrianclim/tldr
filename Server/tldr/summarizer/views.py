from rest_framework.views import APIView
from summarizer.analysis import extract_summary, extract_key_phrases
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


class KeyPhraseView(APIView):
    permission_classes = [AllowAny]
    """
    Get Key Phrases
    """
    def post(self, request):
        key_phrases = extract_key_phrases(request.data['content'])
        return Response({'key_phrases': key_phrases})
