angular.module("useraccount", ["app"])

    .controller("accountsController", function ($scope, $location, $http, growl) {
        var vm = this;

        vm.RoleList = ["user", "manager", "admin"];

        vm.ChangePassword = {};

        vm.StatusFilter = "active";

        $scope.StatusMatch = function (acc) {
            if (vm.StatusFilter === "all") {
                return true;
            }

            if (vm.StatusFilter === "active") {
                return acc.IsActive;
            } else {
                return !acc.IsActive;
            }
        };


        // Re-arms a modal's form so a reopened modal never shows the previous attempt's errors:
        // $setPristine also clears $submitted, $setUntouched clears the per-field touched flags.
        var ResetForm = function (form) {
            if (form) {
                form.$setPristine();

                form.$setUntouched();
            }
        };

        $scope.Init = function () {
            $http({
                method: "POST",
                url: "/Settings/GetAccounts",
                arguments: { "Content-Type": "application/json" }
            }).then(function (data) {
                vm.AccountList = data.data.accountList;
            });

        };

        $scope.NewAccount = function () {
            vm.ModalHeader = "New";

            // Every bound field is initialised explicitly rather than left absent. A control whose
            // validator failed parks $modelValue at undefined, so a model without the property is
            // not a change, ngModel's watch never fires, and the rejected text stays in the input.
            vm.Modal = { Username: "", Password: null, FirstName: "", LastName: "", Role: "user" };

            ResetForm($scope.AccountForm);

            ShowModal("AccountModal");
        };

        $scope.EditAccount = function (value) {
            vm.ModalHeader = "Edit";

            vm.Modal = angular.copy(value);

            // The grid row carries no password; null (not "") both clears any stale view value and
            // is skipped by [StringLength] server-side, which an empty string would fail.
            vm.Modal.Password = null;

            ResetForm($scope.AccountForm);

            ShowModal("AccountModal");
        };

        $scope.Save = function () {

            // AccountForm carries every presence/format rule (see the modal markup); the messages
            // are rendered under each field, so there is nothing to growl here.
            if ($scope.AccountForm.$invalid) {
                return;
            }

            $http({
                method: "POST",
                url: "/Settings/SaveNewAccount",
                data: {
                    account: vm.Modal,
                    role: vm.Modal.Role
                }
            }).then(function (response) {
                PopUpMessage(response.data);

                $scope.Init();

                if (response.data.message == "Saved") {
                    HideModal("AccountModal");
                }
            });
        };

        /*$("#firstName").keypress(function (event) {
            var inputValue = event.which;

            if (!(inputValue >= 65 && inputValue <= 90) && !(inputValue >= 97 && inputValue <= 122) && inputValue != 32) {
                event.preventDefault();
            }
        });


        $('#lastName').keypress(function (event) {
            var inputValue = event.which;

            if (!(inputValue >= 65 && inputValue <= 90) && !(inputValue >= 97 && inputValue <= 122) && inputValue != 32) {
                event.preventDefault();
            }
        });*/

        $scope.UpdatePassword = function (value) {

            vm.Change = angular.copy(value);

            vm.Change.NewPassword = "";

            vm.Change.ConfirmPassword = "";

            ResetForm($scope.ChangePasswordForm);

            ShowModal("ChangePasswordModal");
        };

        $scope.ChangePassword = function () {

            // The match is the one cross-field rule, shown inline by the modal and re-checked here
            // because it is not part of the form's own validity.
            if ($scope.ChangePasswordForm.$invalid || vm.Change.NewPassword !== vm.Change.ConfirmPassword) {
                return;
            }

            $http({
                method: "POST",
                url: "/Settings/AdminChangePassword",
                data: {
                    account: vm.Change.ID,
                    password: vm.Change.NewPassword
                }

            }).then(function (response) {
                if (response.data.errorMessage == "") {
                    growl.success("Password Successfully Changed");

                    $scope.Init();

                    HideModal("ChangePasswordModal");
                }
                else {
                    growl.error(response.data.errorMessage)

                    vm.Change.NewPassword = "";

                    vm.Change.ConfirmPassword = "";

                    ResetForm($scope.ChangePasswordForm);
                }
            });
        };

        $scope.UpdateStatus = function (value) {

            vm.Status = angular.copy(value);

            vm.Status.ConfirmPassword = "";

            ResetForm($scope.StatusForm);

            ShowModal("UpdateStatusModal");
        };

        $scope.SaveStatus = function () {

            if ($scope.StatusForm.$invalid) {
                return;
            }

            $http({
                method: "POST",
                url: "/Settings/UpdateStatus",
                data: {
                    account: vm.Status.ID,
                    password: vm.Status.ConfirmPassword
                }
            }).then(function (response) {
                if (response.data.errorMessage == "") {
                    growl.success("Account Status Successfully Changed");

                    $scope.Init();

                    HideModal("UpdateStatusModal");
                }
                else {
                    growl.error(response.data.errorMessage)

                    vm.Status.ConfirmPassword = "";

                    ResetForm($scope.StatusForm);
                }
            });
        }
    });
