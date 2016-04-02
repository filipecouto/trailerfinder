function Config() {

    var serverUrl = "http://localhost:56559/api";

    this.getServerUrl = function () {
        return serverUrl;
    }
}

var ConfigController = new Config();