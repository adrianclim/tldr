from tldr import secret
from urllib.request import Request, urlopen
import json
from summarizer import summary_tool


AZURE_URL = "https://westus.api.cognitive.microsoft.com/text/analytics/v2.0/keyPhrases"
BING_URL = "https://api.cognitive.microsoft.com/bing/v5.0/search"

headers = {'Content-Type': 'application/json', 'Ocp-Apim-Subscription-Key': secret.AZURE_SUB_KEY}
bing_headers = {'Content-Type': 'application/json', 'Ocp-Apim-Subscription-Key': secret.BING_SUB_KEY}


def extract_key_phrases(text):
    input_text = {"documents": [{"id": "1", "text": text}]}

    # Detect key phrases.
    request = Request(AZURE_URL,
                      data=json.dumps(input_text).encode('utf-8'),
                      headers=headers)

    response = urlopen(request)
    result = response.read().decode()
    print(result)
    obj = json.loads(result)
    print(obj)
    output = obj['documents'][0]['keyPhrases']

    return output


def search_key_phrase(key_phrase):
    search_url = BING_URL + "?q="
    complete_url = search_url + "+".join(key_phrase.split(" ")) + "&count=1"
    request = Request(complete_url,
                      headers=bing_headers)
    response = urlopen(request)
    result = response.read()
    obj = json.loads(result)
    return {'short': obj['webPages']['value'][0]['name'],
            'url': obj['webPages']['value'][0]['url']}


def extract_summary(title, text):
    st = summary_tool.SummaryTool()
    sentences_dic = st.get_senteces_ranks(text)
    summary = st.get_summary(title, text, sentences_dic)
    return summary
