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

        // Re-arms the modal's form: $setPristine also clears $submitted, $setUntouched clears the
        // per-field touched flags, so a reopened modal shows no errors from the previous attempt.
        var ResetForm = function (form) {
            if (form) {
                form.$setPristine();

                form.$setUntouched();
            }
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

            // PasswordForm renders each message under its field. The match is the one cross-field
            // rule and is not part of the form's own validity, so it is re-checked here.
            if ($scope.PasswordForm.$invalid || value.ConfirmPassword !== value.NewPassword) {
                return;
            }

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

                    ResetForm($scope.PasswordForm);
                }
            });
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

            ResetForm($scope.PasswordForm);
        };

    }]);
