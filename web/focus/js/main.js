const queryString = window.location.search;
const urlParams = new URLSearchParams(queryString);
let utm = "none"
if (urlParams.has('utm')) { 
	utm = urlParams.get('utm');
}
const formUrl = "https://docs.google.com/forms/d/e/1FAIpQLScLqSfOZIiZ21DSd0Xdx0TwjvZP_6_-UlC_HFJV5Lu2OouvOA/viewform?entry.947132562="+utm
const formLink = document.getElementById("formLink");
formLink.href = formUrl;
