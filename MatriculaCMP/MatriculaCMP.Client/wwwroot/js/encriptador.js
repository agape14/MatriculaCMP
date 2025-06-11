window.encriptar = function (texto, clave) {
    const key = CryptoJS.enc.Utf8.parse(clave.padEnd(32, '0')); // AES 256 (32 bytes)
    const iv = CryptoJS.lib.WordArray.random(16); // 16 bytes

    const encrypted = CryptoJS.AES.encrypt(texto, key, {
        iv: iv,
        mode: CryptoJS.mode.CBC,
        padding: CryptoJS.pad.Pkcs7
    });

    // Combinar IV y cipherText (en bytes reales)
    const combined = CryptoJS.lib.WordArray.create(iv.words.concat(encrypted.ciphertext.words), iv.sigBytes + encrypted.ciphertext.sigBytes);
    return CryptoJS.enc.Base64.stringify(combined);
};

console.log(">>> Script encriptador cargado");
