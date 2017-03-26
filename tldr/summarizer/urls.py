from summarizer import views
from django.conf.urls import url

urlpatterns = [
    url(r'^summarize$', views.AnalysisRunView.as_view(), name="summarize"),
    url(r'^summarizeurl', views.UrlAnalysisView.as_view(), name="summarize_url"),
    url(r'^keyphrase$', views.KeyPhraseView.as_view(), name="keyextract"),
]