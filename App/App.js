var app = angular.module('app', ["angular-growl", "growlConfig", "login", "useraccount"])
    .controller("mainController", ['$scope', '$location', '$http', 'growl', function ($scope, $location, $http, growl) {
        var main = this;

        main.ChangePassword = {};

        main.ChangePassword.CurrentPassword = "";

        main.ChangePassword.NewPassword = "";

        main.ChangePassword.ConfirmPassword = "";

        main.SearchBox = "";

        main.ItemLoad = true;

        PopUpMessage = function (data) {
            if (data.message == "Saved" || data.message == "Updated" || data.message == "Deleted") {
                growl.success("Successfully " + data.message);
            }
            else {
                growl.error(data.message);
            }
        };

        ShowModal = function (id) {
            coreui.Modal.getOrCreateInstance(document.getElementById(id)).show();
        };

        HideModal = function (id) {
            coreui.Modal.getOrCreateInstance(document.getElementById(id)).hide();
        };

        $scope.Init = function () {
            main.ItemLoad = false;
            $http({
                method: "POST",
                url: "/Home/GetCurrentUser",
                arguments: { "Content-Type": "application/json" }
            }).then(function (data) {
                main.CurrentUser = data.data.obj;

                main.ItemLoad = true;
            });
        };

        $scope.ChangePassword = function (value) {
            if (!value.NewPassword || value.NewPassword.length < 6) {
                growl.error("Password must be at least 6 characters");
            }
            else if (value.ConfirmPassword != value.NewPassword) {
                growl.error("Password Not Match!");

                value.CurrentPassword = "";

                value.NewPassword = "";

                value.ConfirmPassword = "";
            }
            else {
                $http({
                    method: "POST",
                    url: "/Home/ChangePassword",
                    data: { password: value }
                }).then(function (data) {
                    if (data.data.errorMessage == "") {
                        growl.success("Password Successfully Changed");

                        HideModal("PasswordModal");
                    }
                    else {
                        growl.error(data.data.errorMessage);

                        value.CurrentPassword = "";

                        value.NewPassword = "";

                        value.ConfirmPassword = "";
                    }
                });
            }
        };

        $scope.Logout = function () {
            $http({
                method: "POST",
                url: "/Home/Logout",
                arguments: { "Content-Type": "application/json" }
            }).then(function (data) {
                if (data.data != "") {
                    growl.error(data.data);
                }
                else {
                    HideModal("logoutModal");

                    window.location.href = "/Home/Login";
                }
            });
        };

        $scope.OpenPasswordModal = function () {
            main.ChangePassword.CurrentPassword = "";

            main.ChangePassword.NewPassword = "";

            main.ChangePassword.ConfirmPassword = "";
        };

    }]);
