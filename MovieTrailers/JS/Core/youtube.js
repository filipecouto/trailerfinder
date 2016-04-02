var tag = document.createElement('script');

tag.src = "https://www.youtube.com/iframe_api";
var firstScriptTag = document.getElementsByTagName('script')[0];
firstScriptTag.parentNode.insertBefore(tag, firstScriptTag);

var youtubeControllerClass = function () {
    var players = [];

    this.cleanVideos = function () {
        players = [];
    }

    this.updateVideos = function () {
        this.cleanVideos();

        var videoList = document.getElementsByClassName("movies-list")[0];
        var videoElements = videoList.getElementsByClassName("movie-trailer");

        for (var x = 0; x < videoElements.length; x++) {
            var videoElement = videoElements[x];
            var videoId = videoElement.getAttribute("data-video-id");
            var player = new YT.Player(videoElement, {
                height: '390',
                width: '640',
                videoId: videoId,
            });

            players.push(player);
        }

        MoviesController.removeListeners();
        MoviesController.addListeners();
    }
}

var youtubeController = new youtubeControllerClass();