var MoviesControllerClass = function () {

    this.removeListeners = function () {
        var listenerElements = document.getElementsByClassName("facebook-share");

        for (var x = 0; x < listenerElements.length; x++) {
            listenerElements[x].removeEventListener("click", this.share);
        }
    }

    this.addListeners = function () {
        var listenerElements = document.getElementsByClassName("facebook-share");

        for (var x = 0; x < listenerElements.length; x++) {
            listenerElements[x].addEventListener("click", this.share);
        }
    }

    this.cleanVideos = function () {

    }

    this.searchVideos = function (request) {
        var response = request.getResponse();

        var jsonResponse = JSON.parse(response);

        var MovieTemplate = document.getElementById("movie-clone-content").firstElementChild;
        var moviesList = document.getElementsByClassName("movies-list")[0];
        moviesList.innerHTML = "";

        for (var x = 0; x < jsonResponse.length; x++) {
            console.log(jsonResponse[x]);
            var clonedTemplate = MovieTemplate.cloneNode(true);

            var movieTitle = jsonResponse[x].movieData._title;
            var movieYear = jsonResponse[x].movieData._year;
            var moviePlot = jsonResponse[x].movieData._plot;
            var youtubeId = jsonResponse[x].youtubeData.videoId;
    
            var movieTitleDiv = clonedTemplate.getElementsByClassName("movie-title")[0];
            var moviePlotDiv = clonedTemplate.getElementsByClassName("movie-plot")[0];
            var movieTrailer = clonedTemplate.getElementsByClassName("movie-trailer")[0];

            movieTitleDiv.textContent = movieTitle + " - " + movieYear;
            moviePlotDiv.textContent = moviePlot;
            movieTrailer.setAttribute("data-video-id", youtubeId);

            moviesList.appendChild(clonedTemplate);
        }

        youtubeController.updateVideos();
    }

    this.share = function (event) {
        var element = event.target;
        var relevantElement = element.parentNode.parentNode;

        var videoTitle = relevantElement.getElementsByClassName("movie-title")[0].textContent;
        var videoPlot = relevantElement.getElementsByClassName("movie-plot")[0].textContent;
        var videoId = relevantElement.getElementsByClassName("movie-trailer")[0].getAttribute("data-video-id");
        var youtubeUrl = "http://www.youtube.com/watch?v=" + videoId;

        FB.ui({
            method: 'share',
            href: youtubeUrl,
            name: videoTitle,
            caption: videoPlot,
        }, function (response) { });
    }
}

var MoviesController = new MoviesControllerClass();