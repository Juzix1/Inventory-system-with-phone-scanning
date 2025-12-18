window.uploadFileById = async function (fileInputId, url) {
    try {
        const input = document.getElementById(fileInputId);
        if (!input || !input.files || input.files.length === 0) {
            console.warn('uploadFileById: no file selected for', fileInputId);
            return { success: false, message: 'No file selected' };
        }
        const file = input.files[0];
        console.log('uploadFileById: selected file', file.name, file.size, file.type);
        const formData = new FormData();
        formData.append('file', file);
        console.log('uploadFileById: POST', url);

        const resp = await fetch(url, {
            method: 'POST',
            body: formData
        });
        const contentType = resp.headers.get('content-type') || '';
        let body = null;
        try {
            if (contentType.indexOf('application/json') !== -1) {
                body = await resp.json();
            } else {
                body = await resp.text();
            }
        } catch (e) {
            body = await resp.text();
        }
        console.log('uploadFileById: response', resp.status, body);
        return { success: resp.ok, status: resp.status, body: body };
    } catch (err) {
        console.error('uploadFileById: error', err);
        return { success: false, message: err.toString() };
    }
};
