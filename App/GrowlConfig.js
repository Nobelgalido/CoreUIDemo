angular.module("growlConfig", ["angular-growl"])
    .config(['growlProvider', function (growlProvider) {
        growlProvider.globalTimeToLive({ success: 3000, error: 5000, warning: 5000, info: 3000 });
    }]);
