angular.module('app', ['angular-growl'])
    .config(function (growlProvider) {
        growlProvider.globalTimeToLive(4000);
        growlProvider.globalPosition('Top-right');
    });