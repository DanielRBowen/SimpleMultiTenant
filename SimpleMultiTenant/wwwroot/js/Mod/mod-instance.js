const modApp = new Vue({
    el: '#mod',
    data() {
        return {
            editTenant: {
                name: '',
                connectionsString: '',
                domainNames: '',
                ipAddresses: ''
            },
            columns: [
                {
                    label: 'Name',
                    field: 'name'
                },
                {
                    label: 'Connection String',
                    field: 'connectionString'
                },
                {
                    label: 'Domain Names',
                    field: 'domainNames'
                },
                {
                    label: 'Ip Addresses',
                    field: 'ipAddresses'
                }
            ],
            rows: [],
            showAlert: false,
            alertMessage: '',
            alertDanger: false
        };
    },
    computed: {
        alertClass() {
            if (this.alertDanger) {
                return 'alert alert-danger alert-dismissible';
            } else {
                return 'alert alert-success alert-dismissible';
            }
        }
    },
    methods: {
        createTenant() {
            this.disableButtons(true);
            axios.post(tenantPath + '/api/mod/createnewtenant', this.editTenant)
                .then(response => {
                    this.onSuccess(response);
                })
                .catch(error => {
                    this.onFail(error);
                });
        },
        deleteTenant() {
            this.disableButtons(true);
            axios.delete(tenantPath + '/api/mod/deletetenant/' + this.editTenant.name)
                .then(response => {
                    this.onSuccess(response);
                })
                .catch(error => {
                    this.onFail(error);
                });
        },
        updateDomainNames() {
            this.disableButtons(true);
            axios.post(tenantPath + '/api/mod/updatedomainnames', this.editTenant)
                .then(response => {
                    this.onSuccess(response);
                })
                .catch(error => {
                    this.onFail(error);
                });
        },
        updateIpAddresses() {
            this.disableButtons(true);
            axios.post(tenantPath + '/api/mod/updateipaddresses', this.editTenant)
                .then(response => {
                    this.onSuccess(response);
                })
                .catch(error => {
                    this.onFail(error);
                });
        },
        disableButtons(disabled = true) {
            this.$refs.createButton.disabled = disabled;
            this.$refs.deleteButton.disabled = disabled;
            this.$refs.updateDomainNamesButton.disabled = disabled;
            this.$refs.updateIpAddressesButton.disabled = disabled;
        },
        onSuccess(response) {
            this.disableButtons(false);
            this.alertMessage = response.data;
            this.displayAlert();
            this.getTenants();
        },
        onFail(error) {
            this.disableButtons(false);
            this.alertMessage = error.response.data;
            this.displayAlert(true);
            console.log(error);
            this.getTenants();
        },
        getTenants() {
            axios.get(tenantPath + '/api/mod/gettenants')
                .then(response => {
                    newRows = response.data.map((row) => {
                        return {
                            name: row.Name,
                            connectionString: row.ConnectionString,
                            domainNames: row.DomainNames,
                            ipAddresses: row.IpAddresses
                        };
                    });
                    this.rows = newRows;
                })
                .catch(error => {
                    this.alertMessage = error.response.data;
                    this.displayAlert(true);
                    console.log(error);
                });
        },
        displayAlert(isDanger = false) {
            this.alertDanger = isDanger;
            this.showAlert = true;
            let self = this;
            setTimeout(() => {
                self.showAlert = false;
            }, 8000);
        }

    },
    mounted() {
        this.getTenants();
    }
});