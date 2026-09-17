angular.module('app').controller('UsersListController', function ($http, growl) {
    var vm = this;

    vm.users = [];
    vm.filteredUsers = [];
    vm.filter = 'active'; // 'active' | 'inactive' | 'all'
    vm.pageNumber = 1;
    vm.pageSize = 25;

    vm.modalHeader = 'New'; // 'New' | 'Edit' — drives the shared Create/Edit modal
    vm.modal = {};
    vm.errors = [];
    vm.saving = false;

    vm.setFilter = function (filter) {
        vm.filter = filter;
        applyFilter();
    };

    function applyFilter() {
        if (vm.filter === 'active') {
            vm.filteredUsers = vm.users.filter(function (u) { return u.IsActive; });
        } else if (vm.filter === 'inactive') {
            vm.filteredUsers = vm.users.filter(function (u) { return !u.IsActive; });
        } else {
            vm.filteredUsers = vm.users;
        }
    }

    // CoreUI's global is `coreui`, not `bootstrap` — see § 9's Rules and main.controller.js above.
    function userModal() {
        return coreui.Modal.getOrCreateInstance(document.getElementById('UserModal'));
    }

    vm.loadUsers = function () {
        $http.get('/Users/GetUsers', { params: { pageNumber: vm.pageNumber, pageSize: vm.pageSize } })
            .then(function (response) {
                vm.users = response.data;
                applyFilter();
            }, function () {
                growl.error('Failed to load users.');
            });
    };

    vm.newUser = function () {
        vm.modalHeader = 'New';
        vm.errors = [];
        vm.modal = { Username: '', Password: '', FirstName: '', LastName: '', Role: 'user' };
        userModal().show();
    };

    vm.editUser = function (u) {
        vm.modalHeader = 'Edit';
        vm.errors = [];
        // Reuse the row's already-fetched data rather than a round-trip to GetUserById —
        // it's the same data the grid is already showing.
        vm.modal = { Id: u.Id, Username: u.Username, FirstName: u.FirstName, LastName: u.LastName, Role: u.Role, IsActive: u.IsActive };
        userModal().show();
    };

    vm.save = function () {
        vm.errors = [];
        vm.saving = true;

        var url = vm.modalHeader === 'New' ? '/Users/Create' : '/Users/Edit';

        $http.post(url, vm.modal).then(function (response) {
            vm.saving = false;
            if (response.data.success) {
                growl.success(vm.modalHeader === 'New' ? 'User created.' : 'User updated.');
                userModal().hide();
                vm.loadUsers();
            } else {
                vm.errors = response.data.errors || ['Could not save user.'];
            }
        }, function () {
            vm.saving = false;
            vm.errors = ['Server error while saving user.'];
        });
    };

    vm.deleteUser = function (userId) {
        if (!confirm('Deactivate this user?')) return;

        $http.post('/Users/Delete', { userId: userId })
            .then(function (response) {
                if (response.data.success) {
                    growl.success('User deactivated.');
                    vm.loadUsers();
                } else {
                    growl.error('Could not deactivate user.');
                }
            }, function () {
                growl.error('Server error while deleting user.');
            });
    };

    vm.loadUsers();
});