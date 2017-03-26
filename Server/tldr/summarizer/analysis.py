from tldr import secret
from urllib.request import Request, urlopen
import json
from summarizer import summary_tool


AZURE_URL = "https://westus.api.cognitive.microsoft.com/text/analytics/v2.0/keyPhrases"

headers = {'Content-Type': 'application/json', 'Ocp-Apim-Subscription-Key': secret.AZURE_SUB_KEY}


def extract_key_phrases(text):
    input_text = {"documents": [{"id": "1", "text": text}]}

    # Detect key phrases.
    request = Request(AZURE_URL,
                      data=json.dumps(input_text).encode('utf-8'),
                      headers=headers)

    response = urlopen(request)
    result = response.read()
    obj = json.loads(result)['documents'][0]['keyPhrases']

    return obj


def extract_summary(title, text):
    st = summary_tool.SummaryTool()
    sentences_dic = st.get_senteces_ranks(text)
    summary = st.get_summary(title, text, sentences_dic)
    return summary
