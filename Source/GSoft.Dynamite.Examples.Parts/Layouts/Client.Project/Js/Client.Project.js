// Setup the namespace (use existing if any)
window.Client = window.Client || {};
window.GSoft.Dynamite.Examples = window.GSoft.Dynamite.Examples || {};

// ====================
// Basic Wall module
// ====================
(function (BasicWall, $, undefined) {
    // Private properties
    var examplePrivateProperty = "Hello private property!";

    // Public properties
    BasicWall.ExamplePublicProperty = "Hello public property!";

    // Public methods
    BasicWall.initialize = function () {
        $(document).ready(function () {
            $(".new-wall-reply-open-form a").click(function () {
                $(this).parent().siblings(".new-wall-reply").show();
                $(this).parent().hide();
            });
        });
    };

    // Private methods
    function examplePrivateMethod() {
        alert("Hello private method!");
    }

} (GSoft.Dynamite.Examples.BasicWall = GSoft.Dynamite.Examples.BasicWall || {}, jq171));  // Self-executing function - Note how the no-conflit jQuery 
                                                                        // is passed to the module, letting us use $ inside the module

