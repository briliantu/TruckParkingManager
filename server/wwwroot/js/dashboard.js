function formateazaDurata(dataIntrare) {
    const start = new Date(dataIntrare);
    const acum = new Date();
    let diffMs = acum - start;
    if (diffMs < 0) diffMs = 0;

    const minuteTotale = Math.floor(diffMs / 60000);
    const ore = Math.floor(minuteTotale / 60);
    const minute = minuteTotale % 60;
    const zile = Math.floor(ore / 24);
    const oreRest = ore % 24;

    let rezultat = "";
    if (zile > 0) rezultat += zile + " zile, ";
    rezultat += oreRest + " ore, " + minute + " min";
    return rezultat;
}

async function actualizeaza() {
    const corpTabel = document.getElementById('corpTabel');
    const contor = document.getElementById('contor');
    try {
        const raspuns = await fetch('/parcare/active');
        const camioane = await raspuns.json();

        contor.textContent = camioane.length + " camioane active în parcare";

        if (camioane.length === 0) {
            corpTabel.innerHTML = '<tr><td colspan="5" class="gol">Parcarea este goală</td></tr>';
            return;
        }

        corpTabel.innerHTML = camioane
            .sort((a, b) => a.numarLoc - b.numarLoc)
            .map(c => `
                <tr>
                    <td>${c.numarInmatriculare}</td>
                    <td>${c.tara}</td>
                    <td><span class="loc">Loc ${c.numarLoc}</span></td>
                    <td>${new Date(c.dataIntrare).toLocaleString('ro-RO')}</td>
                    <td>${formateazaDurata(c.dataIntrare)}</td>
                </tr>
            `).join('');
    } catch (eroare) {
        corpTabel.innerHTML = '<tr><td colspan="5" class="gol eroare">Nu se poate contacta serverul</td></tr>';
    }
}

actualizeaza();
setInterval(actualizeaza, 5000);
