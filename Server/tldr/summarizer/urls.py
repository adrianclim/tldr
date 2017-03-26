from summarizer import views
from django.conf.urls import url

urlpatterns = [
    url(r'^$', views.AnalysisRunView.as_view(), name="summarize"),
]