angular.module("login", ["angular-growl", "growlConfig"])
    .controller("loginController", ['$scope', '$location', '$http', 'growl', function ($scope, $location, $http, growl) {
        var vm = this;

        $(document).on('keypress', function (e) {
            if (e.which == 13) {
                $scope.TryLogin();
            }
        });

        $scope.TryLogin = function () {
            $http({
                method: "POST",
                url: "/Home/Login",
                data: {
                    username: vm.Username,
                    password: vm.Password
                }
            }).then(function (data) {
                if (data.data.errorMessage != "") {
                    growl.error(data.data.errorMessage);
                }
                else {
                    window.location.href = "/Home/Index";
                }
            });
        };
    }]);
