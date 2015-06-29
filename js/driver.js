function Init() {
    console.log("Fetching xml...");
    $.get("GSoft.Dynamite.xml").success(XmlLoaded);
}

function StartTimer() {
    window.timer = new Date().getTime();
}

function StopTimer(msg) {
    var end = new Date().getTime();
    var time = end - window.timer;
    console.log("[" + time + " ms] : " + msg);
}

function XmlLoaded(response) {
    console.log("... DONE.");
    window.doc = response;
    StartTimer();
    window.vm = new CobraViewModel(response);
    StopTimer("new CobraViewModel");
    ko.applyBindings(window.vm);
}

function CobraViewModel(doc) {
    var self = this;

    self.Doc = doc;
    self.Types = ko.observableArray();
    self.SelectedType = ko.observable();

    // Parse Types
    _.each($(doc).find("member[name^='T:']"), function (type) {
        self.Types.push(new TypeViewModel(type));
    });

    self.SelectedType = ko.observable(self.Types()[0]);

    self.SelectTypeByName = function (typeName) {
        var typeToSelect = _.find(self.Types(), function (type) {
            return type.Name() === typeName;
        });

        self.SelectedType(typeToSelect);
    };
}

// Generic Methods

function ParseSimpleName(fullname) {
    var result = /.*\.(\w*)/.exec(fullname);

    if (result && result.length > 1) {
        return /.*\.(\w*)/.exec(fullname)[1];
    }

    return fullname;
}

function ParseSummary(list, element) {
    var summary = $(element).find("summary");
    var paras = $(summary).find("para");

    if (paras.length > 0) {
        _.each(paras, function (para) {
            list.push($(para).text());
        });
    }
    else {
        list.push((summary).text());
    }
}

// View Models

function TypeViewModel(type) {
    var self = this;
    var fullname = $(type).attr("name");

    self.Name = ko.observable(ParseSimpleName(fullname));
    self.Namespace = ko.observable(Namespace(type));
    self.Summaries = ko.observableArray();
    self.Methods = ko.observableArray();
    self.Properties = ko.observableArray();

    self.OnClick = function () {
        window.vm.SelectedType(self);
    };

    ParseSummary(self.Summaries, type);

    _.each($(doc).find("member[name*='." + self.Name() + ".']"), function (method) {
        if ($(method).attr("name").startsWith("M:")) {
            self.Methods.push(new MethodViewModel(method, self.Name()));
        }
        else if ($(method).attr("name").startsWith("P:")) {
            self.Properties.push(new PropertyViewModel(method));
        }
    });

    function Namespace(type) {
        return /T:(.*)\..*/.exec($(type).attr("name"))[1];
    }
}

function MethodViewModel(method, typeName) {
    var self = this;
    var fullname = $(method).attr("name");

    self.Tag = ko.observable();
    self.FullName = ko.observable(fullname);
    self.ParameterTypes = ko.observableArray();
    self.Parameters = ko.observableArray();
    self.Summaries = ko.observableArray();
    self.Returns = ko.observable($(method).find("returns").text());
    self.Name = ko.observable(MethodName(method));

    self.DisplayTag = ko.computed(function () {
        return "<span class='label label-success'>" + self.Tag() + "</span>";
    });

    self.DisplayName = ko.computed(function () {
        var parameters = [];

        _.each(self.Parameters(), function (parameter) {
            parameters.push("<span class='method-parameter'>" + parameter.TypeAndName() + "</span>");
        });

        return self.Name() + " (<br>" + parameters.join(",<br>") + "<br>)";
    });

    ParseSummary(self.Summaries, method);

    function MethodName(method) {
        var name;

        // 1) Set the raw name
        if (fullname.indexOf("(") > -1) {
            name = /.*\.([`#\w]*)\(/.exec(fullname)[1];

            var parameters = /.*\(([\w.,`{}\[\]@]*)\)/.exec(fullname)[1];
            _.each(parameters.split(","), function (fullType) {
                self.ParameterTypes.push(fullType);
            });
        }
        else {
            name = ParseSimpleName(fullname);
        }

        // 2) Transcode particularities
        if (name === "#ctor") {
            // Constructor
            name = typeName;
            self.Tag("constructor");
        }
        else if (name.indexOf("``") > -1) {
            // Generic Types
            var numberOfGeneric = /``(\d)/.exec(name)[1];
            var asciiU = "U".charCodeAt(0);
            var generics = "<T";
            var firstGenerics = numberOfGeneric - 1;

            _.times(numberOfGeneric - 1, function (index) {
                generics = generics + ", " + String.fromCharCode(asciiU + index);
            });

            name = name.replace("``" + numberOfGeneric, generics + ">");
        }

        return name;
    }

    _.each($(method).find("param"), function (parameter, index) {
        self.Parameters.push(new ParameterViewModel(parameter, self.ParameterTypes()[index]));
    });
}

function PropertyViewModel(property) {
    var self = this;
    var fullname = $(property).attr("name");

    self.Name = ko.observable(ParseSimpleName(fullname));
    self.Summaries = ko.observableArray();

    function PropertyName(property) {
        return /.*\.(\w*)/.exec(fullname)[1];
    }

    ParseSummary(self.Summaries, property);
}

function ParameterViewModel(parameter, fullType) {
    var self = this;

    self.Name = ko.observable($(parameter).attr("name"));
    self.Description = ko.observable($(parameter).text());
    self.FullType = ko.observable(fullType);
    self.Type = ko.computed(function () {
        return ParseSimpleName(self.FullType());
    });

    self.TypeAndName = ko.computed(function () {
        return "<span class='parameter-type'>" + self.Type() + "</span>&nbsp;<span class='parameter-name'>" + self.Name() + "</span>";
    });

    self.IsDynamiteType = ko.computed(function () {
        return self.FullType().indexOf("GSoft") === 0;
    });

    self.OnFullTypeClick = function () {
        window.vm.SelectTypeByName(self.Type());
    };
}

// Run
Init();
