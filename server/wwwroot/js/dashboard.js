const CAPACITATE_TOTALA = 50;
let camioaneActive = [];
let camioaneToate = [];

let esteAdmin = false;
const PAROLA_ADMIN = "admin123";

function deschideModalLogin() {
    if (esteAdmin) {
        esteAdmin = false;
        document.getElementById('adminControls').style.display = 'none';
        document.getElementById('historySection').style.display = 'none';
        document.getElementById('filterSection').style.display = 'none';
        document.getElementById('btnLogin').innerText = "🔑 Login Admin";
        document.getElementById('btnLogin').className = "btn btn-warning";
        aplicaFiltre();
        alert("Te-ai delogat din contul de Admin.");
    } else {
        document.getElementById('inputParolaAdmin').value = '';
        document.getElementById('modalLogin').style.display = 'flex';
        document.getElementById('inputParolaAdmin').focus();
    }
}

function inchideModalLogin() {
    document.getElementById('modalLogin').style.display = 'none';
}

function executaLoginAdmin() {
    const parola = document.getElementById('inputParolaAdmin').value;
    if (parola === PAROLA_ADMIN) {
        esteAdmin = true;
        document.getElementById('adminControls').style.display = 'inline-block';
        document.getElementById('historySection').style.display = 'block';
        document.getElementById('filterSection').style.display = 'block';

        document.getElementById('btnLogin').innerText = "🔓 Logout Admin";
        document.getElementById('btnLogin').className = "btn btn-danger";

        inchideModalLogin();
        renderHistoryTable();
        alert("Autentificare reușită ca Admin!");
    } else {
        alert("Parolă incorectă!");
    }
}

function formateazaDurataLive(dataIntrare) {
    const start = new Date(dataIntrare);
    const acum = new Date();
    let diffMs = acum - start;
    if (diffMs < 0) diffMs = 0;

    const totalSecs = Math.floor(diffMs / 1000);
    const ore = Math.floor(totalSecs / 3600);
    const minute = Math.floor((totalSecs % 3600) / 60);
    const secunde = totalSecs % 60;

    return `${ore.toString().padStart(2, '0')}h ${minute.toString().padStart(2, '0')}m ${secunde.toString().padStart(2, '0')}s`;
}

async function incarcaStats() {
    try {
        const r = await fetch('/parcare/stats');
        const stats = await r.json();

        document.getElementById('kpiLibere').innerText = stats.capacitate - stats.ocupate;
        document.getElementById('kpiOcupate').innerText = stats.ocupate;
        document.getElementById('kpiTotal').innerText = stats.capacitate;
        document.getElementById('kpiCumulat').innerText = stats.totalCumulat;
    } catch (e) { }
}

async function incarcaCamioane() {
    try {
        const rActive = await fetch('/parcare/active');
        camioaneActive = await rActive.json();

        const rToate = await fetch('/parcare/toate');
        camioaneToate = await rToate.json();

        renderMap();
        aplicaFiltre();
    } catch (e) {
        document.getElementById('corpTabel').innerHTML = '<tr><td colspan="5" class="gol">Nu se poate contacta serverul.</td></tr>';
    }
}

// --- DESENARE PARCARE 5 SECTOARE X 10 LOCURI ---
function renderMap() {
    const rowElements = [
        document.getElementById('parkingRow1'),
        document.getElementById('parkingRow2'),
        document.getElementById('parkingRow3'),
        document.getElementById('parkingRow4'),
        document.getElementById('parkingRow5')
    ];

    rowElements.forEach(r => r ? r.innerHTML = '' : null);

    const ocupateSet = new Map();
    camioaneActive.forEach(c => ocupateSet.set(c.numarLoc, c));

    for (let i = 1; i <= CAPACITATE_TOTALA; i++) {
        const camion = ocupateSet.get(i);
        const esteOcupat = !!camion;

        const slot = document.createElement('div');
        slot.className = `top-slot ${esteOcupat ? 'occupied' : 'free'}`;

        if (esteOcupat) {
            slot.innerHTML = `
                <div class="slot-label">#${i < 10 ? '0' + i : i}</div>
                <div class="truck-topview">
                    <div class="truck-cab">
                        <div class="truck-windshield"></div>
                    </div>
                    <div class="truck-body">
                        <span class="truck-body-plate">${camion.numarInmatriculare}</span>
                        <span class="truck-body-country">${camion.tara}</span>
                    </div>
                </div>
            `;
        } else {
            slot.innerHTML = `
                <div class="slot-label">#${i < 10 ? '0' + i : i}</div>
                <div class="free-mark">P</div>
            `;
        }

        const rowIndex = Math.floor((i - 1) / 10);
        if (rowElements[rowIndex]) {
            rowElements[rowIndex].appendChild(slot);
        }
    }
}

function aplicaFiltre() {
    const tara = document.getElementById('filtruTara').value;
    const startData = document.getElementById('filtruDataStart').value;
    const endData = document.getElementById('filtruDataEnd').value;

    let activeFiltrate = camioaneActive.filter(c => {
        if (tara && c.tara !== tara) return false;
        if (startData) {
            const dIntrare = new Date(c.dataIntrare).toISOString().split('T')[0];
            if (dIntrare < startData) return false;
        }
        if (endData) {
            const dIntrare = new Date(c.dataIntrare).toISOString().split('T')[0];
            if (dIntrare > endData) return false;
        }
        return true;
    });

    renderTable(activeFiltrate);

    if (esteAdmin) {
        let toateFiltrate = camioaneToate.filter(c => {
            if (tara && c.tara !== tara) return false;
            if (startData) {
                const dIntrare = new Date(c.dataIntrare).toISOString().split('T')[0];
                if (dIntrare < startData) return false;
            }
            if (endData) {
                const dIntrare = new Date(c.dataIntrare).toISOString().split('T')[0];
                if (dIntrare > endData) return false;
            }
            return true;
        });
        renderHistoryTable(toateFiltrate);
    }
}

function reseteazaFiltre() {
    document.getElementById('filtruTara').value = '';
    document.getElementById('filtruDataStart').value = '';
    document.getElementById('filtruDataEnd').value = '';
    aplicaFiltre();
}

function renderTable(lista) {
    const corpTabel = document.getElementById('corpTabel');
    if (!lista || lista.length === 0) {
        corpTabel.innerHTML = '<tr><td colspan="5" class="gol">Niciun camion găsit conform filtrelor.</td></tr>';
        return;
    }

    corpTabel.innerHTML = lista
        .sort((a, b) => a.numarLoc - b.numarLoc)
        .map(c => `
            <tr>
                <td><strong>LOC #${c.numarLoc < 10 ? '0' + c.numarLoc : c.numarLoc}</strong></td>
                <td><strong>${c.numarInmatriculare}</strong></td>
                <td>${c.tara}</td>
                <td>${new Date(c.dataIntrare).toLocaleString('ro-RO')}</td>
                <td><span style="color:#e67e22; font-weight:bold;">${formateazaDurataLive(c.dataIntrare)}</span></td>
            </tr>
        `).join('');
}

function renderHistoryTable(lista) {
    const corpHistory = document.getElementById('corpTabelIstoric');
    if (!lista || lista.length === 0) {
        corpHistory.innerHTML = '<tr><td colspan="6" class="gol">Nicio înregistrare în istoric conform filtrelor.</td></tr>';
        return;
    }

    corpHistory.innerHTML = lista
        .slice()
        .reverse()
        .map(c => `
            <tr>
                <td><strong>LOC #${c.numarLoc < 10 ? '0' + c.numarLoc : c.numarLoc}</strong></td>
                <td><strong>${c.numarInmatriculare}</strong></td>
                <td>${c.tara}</td>
                <td>${new Date(c.dataIntrare).toLocaleString('ro-RO')}</td>
                <td>${c.dataIesire ? new Date(c.dataIesire).toLocaleString('ro-RO') : '<span style="color:#2ecc71;">ÎN PARCARE</span>'}</td>
                <td>${c.durataTotala || '-'}</td>
            </tr>
        `).join('');
}

async function curataCamioaneActive() {
    if (!esteAdmin) return;
    if (confirm("Ești sigur că vrei să scoți TOATE camioanele active uitate în parcare?")) {
        try {
            const r = await fetch('/parcare/clear-active', { method: 'POST' });
            if (r.ok) {
                alert("Toate camioanele active au fost eliberate!");
                incarcaStats();
                incarcaCamioane();
            }
        } catch (e) {
            alert("Eroare la conectare cu serverul!");
        }
    }
}

function exportToSVG() {
    let svg = `<svg xmlns="http://www.w3.org/2000/svg" width="1000" height="700" style="background:#1e232a; font-family:Arial;">`;
    svg += `<text x="20" y="40" font-size="22" font-weight="bold" fill="#ffffff">De'Longhi - Harta Parcare Camioane</text>`;
    svg += `<text x="20" y="65" font-size="13" fill="#8a99ad">Exportat la: ${new Date().toLocaleString('ro-RO')}</text>`;

    const ocupateSet = new Map();
    camioaneActive.forEach(c => ocupateSet.set(c.numarLoc, c.numarInmatriculare));

    for (let i = 1; i <= CAPACITATE_TOTALA; i++) {
        const col = (i - 1) % 10;
        const row = Math.floor((i - 1) / 10);
        const x = 20 + col * 95;
        const y = 85 + row * 60;

        const esteOcupat = ocupateSet.has(i);
        const color = esteOcupat ? '#e74c3c' : '#2ecc71';
        const plate = esteOcupat ? ocupateSet.get(i) : 'LIBER';

        svg += `
            <g transform="translate(${x}, ${y})">
                <rect width="90" height="50" rx="4" fill="${color}" opacity="0.9"/>
                <text x="8" y="20" fill="#fff" font-weight="bold" font-size="12">LOC #${i < 10 ? '0' + i : i}</text>
                <text x="8" y="38" fill="#fff" font-size="10">${plate}</text>
            </g>
        `;
    }

    svg += `</svg>`;

    const blob = new Blob([svg], { type: 'image/svg+xml' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `Harta_Parcare_DeLonghi.svg`;
    a.click();
}

function exportToCSV() {
    let csv = 'Loc Parcare,Numar Inmatriculare,Tara,Data Intrare,Data Iesire,Durata Totala\n';
    camioaneToate.forEach(c => {
        csv += `"LOC #${c.numarLoc}","${c.numarInmatriculare}","${c.tara}","${c.dataIntrare}","${c.dataIesire || 'PARCAT'}","${c.durataTotala}"\n`;
    });

    const blob = new Blob([csv], { type: 'text/csv' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `Raport_Parcare_DeLonghi.csv`;
    a.click();
}

setInterval(() => {
    incarcaStats();
    incarcaCamioane();
}, 3000);

incarcaStats();
incarcaCamioane();