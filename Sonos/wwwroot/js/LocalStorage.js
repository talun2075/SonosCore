// Funktion zum Speichern eines JSON-Objekts im localStorage mit Änderungsdatum
function setStore(key, value) {
    const dataToStore = {
        value: value,
        lastUpdated: new Date().toISOString() // Änderungsdatum speichern
    };
    localStorage.setItem(key, JSON.stringify(dataToStore));
}

// Funktion zum Abrufen eines Wertes aus dem localStorage
function getStore(key) {

    const data = localStorage.getItem(key);
    if (data) {
        return JSON.parse(data); 
    }
    return null; // Gibt null zurück, wenn kein Wert vorhanden ist
}
function removeStore(key) {
    localStorage.removeItem(key);
}

// Funktion zur Überprüfung, ob eine Aktualisierung erforderlich ist
function isUpdateRequired(lastUpdated) {

    const twentyFourHoursInMilliseconds = 24 * 60 * 60 * 1000 * 7; //Letzten 7 Tage nicht aktualisieren.
    const lastUpdateDate = new Date(lastUpdated);
    return (new Date() - lastUpdateDate) > twentyFourHoursInMilliseconds;
}



