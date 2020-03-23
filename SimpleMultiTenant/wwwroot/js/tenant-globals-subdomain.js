// https://stackoverflow.com/questions/1282726/get-subdomain-and-load-it-to-url-with-greasemonkey
function getSubdomain(hostname) {
    var regexParse = new RegExp('[a-z\-0-9]{2,63}\.[a-z\.]{2,5}$');
    var urlParts = regexParse.exec(hostname);
    return hostname.replace(urlParts[0], '').slice(0, -1);
}

let tenantPath = '';
//let tenantName = getSubdomain(window.location.hostname);
let tenantName = '';

// What if there is no subdomain and there is a domain name or ip address associated with the tenant then get tenant found on the server.
if (typeof tenantName === 'undefined' || tenantName === '') {
    axios.get('api/tenants/gettenantname')
        .then(response => {
            tenantName = response.data;
        })
        .catch(error => {
            console.log(error.response.message);
        });
}