function AjaxRequest(onDataReceived) {
    if (window.XMLHttpRequest) {
        this.request = new XMLHttpRequest();
    } else {
        this.request = new ActiveXObject("Microsoft.XMLHTTP");
    }
    this.request.request = this;

    this.request.onreadystatechange = function () {
        if (this.readyState == 4 && this.status == 200) {
            onDataReceived(this.request);
        }
    }

    this.get = function (url) {
        this.request.open("GET", url, true);
        this.request.send();
    };
    this.post = function (url, data) {
        this.request.open("POST", url, true);
        this.request.setRequestHeader("Content-type", "application/x-www-form-urlencoded");
        this.request.send(data);
    };

    this.getResponse = function () {
        return this.request.responseText;
    }

    this.getXmlResponse = function () {
        return this.request.responseXML;
    }

    this.getJsonResponse = function () {
        return JSON.parse(this.request.responseText);
    }
}

function AsyncFragmentLoader(container) {
    container.asyncFragmentLoader = this;

    this.onDataReceived = function (response) {
        container.innerHTML = response.getResponse();
        var fragmentCode = container.getElementsByTagName('fragmentCode')[0];
        window.eval(fragmentCode.innerText);
        container.removeChild(fragmentCode);
        if (response.onLoaded) response.onLoaded();
    }

    this.load = function (fragment) {
        var request = new AjaxRequest(this.onDataReceived);
        request.onLoaded = this.onLoaded;
        request.post(fragment, '_getFragment=true');
    }
}

HTMLDivElement.prototype.loadFragment = function (url, onLoaded) {
    var loader = this.asyncFragmentLoader;
    if (!loader) loader = new AsyncFragmentLoader(this);
    loader.onLoaded = onLoaded;
    loader.load(url);
}

function getFormQueryString(form) {
    var res = "";

    function getValue(name, value) {
        // res += escape(name) + "=" + escape(value);
        res += encodeURIComponent(name) + "=" + encodeURIComponent(value);
    }

    var fields = form.elements;
    for (var i = 0; i < fields.length; i++) {
        if (i != 0) res += "&";
        var f = fields[i];
        var type = f.type.toLowerCase();
        var name = f.name;
        if (name) {
            if (type == "checkbox") {
                getValue(name, f.checked ? "true" : "false");
            } else if (f.checked && type == "radio") {
                getValue(name, f.value);
            } else if (type == "select") {
                for (var o = 0; o < f.options.length; o++) {
                    var option = f.options[o];
                    if (option.selected) getValue(name, option.value ? option.value : option.text);
                }
            } else {
                getValue(name, f.value);
            }
        }
    }

    return res;
}

function submitFormByAjax(form, onDataReceived) {
    var ajax = new AjaxRequest(onDataReceived);
    ajax.form = form;

    var serverUrl = ConfigController.getServerUrl() + form.getAttribute("data-action");
    ajax.get(serverUrl + "?" + getFormQueryString(form));
    return false;
}