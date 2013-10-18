// No conflict mode frees up the $ symbol.
// The variable jq171 now gives us direct access to jQuery v1.7.1, even if multiple versions were loaded.
// We assume that this script runs immediatly after the jQuery script link.
var jq171 = jQuery.noConflict();