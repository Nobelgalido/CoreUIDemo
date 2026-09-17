angular.module('app').controller('MainController', function ($scope, $http, $window, growl) {
    var main = this;
    main.ItemLoad = false;
    main.CurrentUser = null;
    main.ChangePassword = { CurrentPassword: '', NewPassword: '' };
    main.PasswordErrors = [];
    main.PasswordSaving = false;

    // Called via ng-init="Init()" on the wrapping div — mirrors OneMasaito's pattern of
    // hiding the whole page behind a loader until the current user is known.
    $scope.Init = function () {
        $http.get('/Account/CurrentUserInfo').then(function (response) {
            main.CurrentUser = response.data;
            main.ItemLoad = true;
        });
    };

    // Bound to the Logout modal's confirm button (ng-click="Logout()") — a plain
    // navigation is enough since /Account/Logout just clears the auth cookie server-side.
    $scope.Logout = function () {
        $window.location.href = '/Account/Logout';
    };

    // Bound to the header's Change Password modal, available to every authenticated
    // user regardless of role.
    $scope.ChangePassword = function (model) {
        main.PasswordErrors = [];
        main.PasswordSaving = true;

        $http.post('/Users/ChangePassword', model).then(function (response) {
            main.PasswordSaving = false;
            if (response.data.success) {
                growl.success('Password changed.');
                main.ChangePassword = { CurrentPassword: '', NewPassword: '' };
                passwordModal().hide();
            } else {
                main.PasswordErrors = response.data.errors || ['Could not change password.'];
            }
        }, function () {
            main.PasswordSaving = false;
            main.PasswordErrors = ['Server error while changing password.'];
        });
    };

    // CoreUI's global is `coreui`, not `bootstrap` — coreui.bundle.min.js renames the
    // whole Bootstrap 5 JS API onto this namespace. See § 9's Rules.
    function passwordModal() {
        return coreui.Modal.getOrCreateInstance(document.getElementById('PasswordModal'));
    }
});