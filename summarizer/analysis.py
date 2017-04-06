from tldr import secret
from urllib.request import Request, urlopen
import json
from summarizer import summary_tool
from bs4 import BeautifulSoup as bs4


AZURE_URL = "https://westus.api.cognitive.microsoft.com/text/analytics/v2.0/keyPhrases"
BING_URL = "https://api.cognitive.microsoft.com/bing/v5.0/search"
SPELL_URL = "https://api.cognitive.microsoft.com/bing/v5.0/spellcheck/"

headers = {'Content-Type': 'application/json', 'Ocp-Apim-Subscription-Key': secret.AZURE_SUB_KEY}
bing_headers = {'Content-Type': 'application/json', 'Ocp-Apim-Subscription-Key': secret.BING_SUB_KEY}
spell_headers = {'Content-Type': 'application/x-www-form-urlencoded', 'Ocp-Apim-Subscription-Key': secret.SPELL_SUB_KEY}


def extract_key_phrases(text):
    input_text = {"documents": [{"id": "1", "text": text}]}

    # Detect key phrases.
    request = Request(AZURE_URL,
                      data=json.dumps(input_text).encode('utf-8'),
                      headers=headers)

    response = urlopen(request)
    result = response.read().decode()
    obj = json.loads(result)
    output = obj['documents'][0]['keyPhrases']

    return output


def search_key_phrase(key_phrase):
    search_url = BING_URL + "?q="
    complete_url = search_url + "+".join(key_phrase.split(" ")) + "&count=1"
    request = Request(complete_url,
                      headers=bing_headers)
    response = urlopen(request)
    result = response.read().decode()
    obj = json.loads(result)

    print("Searching key {}".format(key_phrase))

    try:
        if len(obj) <= 0:
            return {
                'short': "Not Found",
                'url': "Not Found"
            }
        else:
            return {
                'short': obj['webPages']['value'][0]['name'],
                'url': obj['webPages']['value'][0]['url']
            }
    except:
        return {
            'short': "Not Found",
            'url': "Not Found"
        }


def spell_correct(mode, text, threshold):
    spell_url = SPELL_URL + "?mode={}".format(mode)

    input_text = "Text={}".format(text)

    request = Request(spell_url,
                      data=input_text.encode("utf-8"),
                      headers=spell_headers)

    response = urlopen(request)
    result = response.read().decode()
    obj = json.loads(result)
    print(obj)

    summary = text
    counter = 0
    outputString = []

    if "flaggedTokens" in obj:
        # Incorrect spelling may have been found
        for i in obj["flaggedTokens"]:
            if i["suggestions"][0]["score"] > threshold:
                current_offset = int(i["offset"])
                outputString.append(summary[counter: current_offset])
                outputString.append(i["suggestions"][0]["suggestion"])
                counter = current_offset + len(i["token"])

    if counter < len(summary) - 1:
        outputString.append(summary[counter: len(summary)])

    return "".join(outputString)


def extract_summary(text):
    st = summary_tool.SummaryTool()
    sentences_dic = st.get_senteces_ranks(text)
    summary = st.get_summary(text, sentences_dic)

    if summary == "":
        return text

    return summary


def scrape_url(url):
    html = urlopen(url).read()
    soup = bs4(html, "html.parser")
    texts = soup.find('p').getText()

    return texts
