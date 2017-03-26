from rest_framework.views import APIView
from summarizer.analysis import extract_summary, extract_key_phrases, search_key_phrase
from rest_framework.response import Response
from rest_framework.permissions import AllowAny


# Create your views here.


class AnalysisRunView(APIView):
    permission_classes = [AllowAny]
    """
    Run an Analysis
    """
    def post(self, request):
        print(request.data['content'] + "\n\n")

        summary = extract_summary(request.data['content'])
        key_phrases = extract_key_phrases(request.data['content'])

        print("summary: " + summary)
        print("key phrases: " + ",".join(key_phrases))

        web_list = []
        for i in key_phrases:
            web = search_key_phrase(i)
            web_list.append({'phrase': i,
                             'short_name': web['short'],
                             'url': web['url']})

        return Response({'summary': summary,
                         'key_phrases': web_list})


class KeyPhraseView(APIView):
    permission_classes = [AllowAny]
    """
    Get Key Phrases
    """
    def post(self, request):
        key_phrases = extract_key_phrases(request.data['content'])
        return Response({'key_phrases': key_phrases})
