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


        var namePattern = /^[a-zA-Z ]+$/;

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

            vm.Modal = { Role: "user" };

            ShowModal("AccountModal");
        };

        $scope.EditAccount = function (value) {
            vm.ModalHeader = "Edit";

            vm.Modal = angular.copy(value);

            ShowModal("AccountModal");
        };

        $scope.Save = function () {
            var FirstName = (vm.Modal.FirstName || "").trim();
            var LastName = (vm.Modal.LastName || "").trim();

            if (!vm.Modal.Username) {
                growl.error("Please input Username");
            }
            else if (vm.ModalHeader === "New" && !vm.Modal.Password) {
                growl.error("Please input Password");
            }
            else if (vm.ModalHeader === "New" && vm.Modal.Password.length < 6) {
                growl.error("Password must be at least 6 characters");
            }
            else if (!FirstName) {
                growl.error("Please input First Name");
            }
            else if (!namePattern.test(FirstName)) {
                growl.error("First Name must contain letters only");
            }
            else if (!LastName) {
                growl.error("Please input Last Name");
            }
            else if (!namePattern.test(LastName)) {
                growl.error("Last Name must contain letters only");
            }
            else if (!vm.Modal.Role) {
                growl.error("Please select Role");
            }
            else {
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
            }
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

            ShowModal("ChangePasswordModal");
        };

        $scope.ChangePassword = function () {

            if (!vm.Change.NewPassword) {
                growl.error("Please input New Password");
            }
            else if (!vm.Change.ConfirmPassword) {
                growl.error("Please input Confirm Password");
            }
            else if (vm.Change.NewPassword.length < 6) {
                growl.error("Password must be at least 6 characters");
            }
            else {
                if (vm.Change.NewPassword != vm.Change.ConfirmPassword) {
                    growl.error("Password Not Match!");
                }
                else {
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
                        }
                    });
                }
            }
        };

        $scope.UpdateStatus = function (value) {

            vm.Status = angular.copy(value);

            ShowModal("UpdateStatusModal");
        };

        $scope.SaveStatus = function () {
            if (!vm.Status.ConfirmPassword) {
                growl.error("Please input Password to proceed");
            }
            else {
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

                    }

                });
            }

        }
    });
