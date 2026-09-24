angular.module("login", ["angular-growl", "growlConfig"])
    .controller("loginController", ['$scope', '$location', '$http', 'growl', function ($scope, $location, $http, growl) {
        var vm = this;

        $scope.TryLogin = function () {

            // LoginForm carries the two required rules and renders their messages under the fields.
            if ($scope.LoginForm.$invalid) {
                return;
            }

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
