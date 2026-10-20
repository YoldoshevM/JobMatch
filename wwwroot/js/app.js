
"use strict";

/* ==========================================
   JOBMATCH - COMPLETE FRONTEND JAVASCRIPT
   ========================================== */

const $ = id => document.getElementById(id);

const pages = document.querySelectorAll(".page");
const navLinks = document.querySelectorAll(".nav-link");

let allVacancies = [];
let allResumes = [];
let lastFocusedElement = null;
let analysisVersion = 0;

const pageNames = {
    dashboard: "Dashboard",
    vacancies: "Vacancies",
    resume: "My Resume",
    results: "Match Results"
};

/* ==========================================
   HELPERS
   ========================================== */

function escapeHtml(value) {
    return String(value ?? "").replace(
        /[&<>"']/g,
        char => ({
            "&": "&amp;",
            "<": "&lt;",
            ">": "&gt;",
            '"': "&quot;",
            "'": "&#39;"
        })[char]
    );
}

function safeHttpUrl(value) {
    if (!value || typeof value !== "string") {
        return "";
    }

    try {
        const url = new URL(value.trim());

        if (!["https:", "http:"].includes(url.protocol)) {
            return "";
        }

        return url.href;
    } catch {
        return "";
    }
}

function isDemoUrl(value) {
    const url = safeHttpUrl(value);

    if (!url) return true;

    const hostname = new URL(url).hostname.toLowerCase();

    return [
        "example.com",
        "example.org",
        "example.net",
        "localhost",
        "127.0.0.1"
    ].some(domain =>
        hostname === domain ||
        hostname.endsWith("." + domain)
    );
}

function getApplicationUrl(vacancy) {
    if (!vacancy) return "";

    // Backendda maydon nomi turlicha bo'lishi mumkin
    const candidates = [
        vacancy.applicationUrl,
        vacancy.applyUrl,
        vacancy.applicationLink,
        vacancy.applyLink
    ];

    for (const candidate of candidates) {
        const url = safeHttpUrl(candidate);

        if (url && !isDemoUrl(url)) {
            return url;
        }
    }

    return "";
}

async function apiGet(url) {
    const response = await fetch(url, {
        headers: {
            Accept: "application/json"
        },
        cache: "no-store"
    });

    if (!response.ok) {
        throw new Error(
            `API error: ${response.status} ${response.statusText}`
        );
    }

    return await response.json();
}

function setApiStatus(connected) {
    const element = $("apiStatus");

    if (!element) return;

    element.classList.toggle("error", !connected);

    element.innerHTML = connected
        ? '<span class="online-dot"></span> API Connected'
        : '<span class="online-dot"></span> API Unavailable';
}

/* ==========================================
   NAVIGATION
   ========================================== */

function showPage(pageId) {
    if (!pageNames[pageId]) {
        pageId = "dashboard";
    }

    pages.forEach(page => {
        page.classList.toggle(
            "active-page",
            page.id === pageId
        );
    });

    navLinks.forEach(link => {
        const active = link.dataset.page === pageId;

        link.classList.toggle("active", active);

        if (active) {
            link.setAttribute("aria-current", "page");
        } else {
            link.removeAttribute("aria-current");
        }
    });

    if ($("pageName")) {
        $("pageName").textContent = pageNames[pageId];
    }

    window.scrollTo(0, 0);
}

function navigate() {
    const pageId =
        location.hash.substring(1) || "dashboard";

    showPage(pageId);

    if (pageId !== "vacancies") {
        closeVacancyDrawer();
    }
}

window.addEventListener("hashchange", navigate);

/* ==========================================
   VACANCY HELPERS
   ========================================== */

function getRequirements(vacancy) {
    return Array.isArray(vacancy?.requirements)
        ? vacancy.requirements
        : [];
}

function getSkillName(requirement) {
    return requirement?.skill?.name ||
        requirement?.skillName ||
        "Unknown skill";
}

function vacancyLabel(vacancy) {
    const company = vacancy.company?.name || "";

    return company
        ? `${vacancy.title} — ${company}`
        : vacancy.title;
}

function renderSkillChips(requirements, optional = false) {
    if (!requirements.length) {
        return '<span class="no-skills">Not specified</span>';
    }

    return requirements.map(item => `
        <span class="skill-chip ${optional ? "optional" : ""}">
            ${escapeHtml(getSkillName(item))}
        </span>
    `).join("");
}

function selectVacancyForAnalysis(vacancyId) {
    const select = $("vacancySelect");

    if (!select) return;

    select.value = String(vacancyId);

    clearMatchResult();
    closeVacancyDrawer();

    location.hash = "results";
    showPage("results");
}

/* ==========================================
   VACANCY CARDS
   ========================================== */

function createVacancyCard(vacancy) {
    const card = document.createElement("article");

    card.className = "glass vacancy-card";

    const company =
        vacancy.company?.name || "Unknown company";

    const locationText =
        vacancy.location || "Location not specified";

    const requirements = getRequirements(vacancy);

    const visibleSkills = requirements.slice(0, 3);

    const remainingSkills = Math.max(
        0,
        requirements.length - 3
    );

    const initials = company
        .trim()
        .split(/\s+/)
        .slice(0, 2)
        .map(word => word[0]?.toUpperCase() || "")
        .join("");

    const applicationUrl = getApplicationUrl(vacancy);

    card.innerHTML = `
        <div class="jm-card-head">
            <div class="jm-company-icon">
                ${escapeHtml(initials)}
            </div>

            <div>
                <div class="jm-card-company">
                    ${escapeHtml(company)}
                </div>

                <div class="jm-card-label">
                    ${applicationUrl
            ? "JOB OPPORTUNITY"
            : "APPLICATION LINK UNAVAILABLE"}
                </div>
            </div>
        </div>

        <h2>${escapeHtml(vacancy.title)}</h2>

        <div class="location-line">
            ◉ ${escapeHtml(locationText)}
        </div>

        <div class="jm-card-skills">
            ${visibleSkills.map(item => `
                <span class="skill-chip ${item.isRequired === false ? "optional" : ""}">
                    ${escapeHtml(getSkillName(item))}
                </span>
            `).join("")}

            ${remainingSkills > 0
            ? `<span class="jm-more-skills">
                    +${remainingSkills} skills
                   </span>`
            : ""}
        </div>

        <div class="jm-card-bottom">
            <button
                type="button"
                class="jm-view-button">
                View Details ↗
            </button>
        </div>
    `;

    card.querySelector(".jm-view-button")
        .addEventListener("click", () => {
            openVacancyDrawer(vacancy);
        });

    return card;
}

function renderVacancies(vacancies) {
    const container = $("vacancyList");

    if (!container) return;

    container.replaceChildren();

    if ($("vacancyResultCount")) {
        $("vacancyResultCount").textContent =
            `${vacancies.length} opportunities found`;
    }

    if (!vacancies.length) {
        container.innerHTML = `
            <div class="glass empty-state">
                No vacancies found.
            </div>
        `;
        return;
    }

    const fragment = document.createDocumentFragment();

    vacancies.forEach(vacancy => {
        fragment.appendChild(createVacancyCard(vacancy));
    });

    container.appendChild(fragment);
}


function searchVacancies() {

    // 1. Foydalanuvchi yozgan matnni olamiz
    const search = document
        .getElementById("vacancySearch")
        .value
        .toLowerCase()
        .trim();

    // 2. Natijalarni saqlash uchun bo'sh array
    let results = [];

    // 3. Barcha vakansiyalarni bittalab tekshiramiz
    for (let vacancy of allVacancies) {

        let title = (vacancy.title || "").toLowerCase();

        let company = (vacancy.company?.name || "")
            .toLowerCase();

        let location = (vacancy.location || "")
            .toLowerCase();

        // Vakansiyadagi skill nomlarini yig'amiz
        let skills = "";

        for (let requirement of vacancy.requirements || []) {

            skills += " " + (
                requirement.skill?.name ||
                requirement.skillName ||
                ""
            ).toLowerCase();
        }

        // 4. Qidirilgan so'z qayerdadir bormi?
        if (
            title.includes(search) ||
            company.includes(search) ||
            location.includes(search) ||
            skills.includes(search)
        ) {
            results.push(vacancy);
        }
    }

    // 5. Mos vakansiyalarni ekranga chiqaramiz
    renderVacancies(results);
}


/* ==========================================
   VACANCY DETAILS DRAWER
   ========================================== */

function openVacancyDrawer(vacancy) {
    const root = $("vacancyDrawer");
    const content = $("jmDrawerContent");

    if (!root || !content) return;

    lastFocusedElement = document.activeElement;

    const requirements = getRequirements(vacancy);

    const required = requirements.filter(
        item => item.isRequired === true
    );

    const optional = requirements.filter(
        item => item.isRequired === false
    );

    const company =
        vacancy.company?.name || "Unknown company";

    const locationText =
        vacancy.location || "Location not specified";

    const applicationUrl = getApplicationUrl(vacancy);

    const salary =
        vacancy.salaryFrom != null ||
            vacancy.salaryTo != null
            ? `${vacancy.salaryFrom ?? "?"} – ${vacancy.salaryTo ?? "?"}`
            : "Not disclosed";

    content.innerHTML = `
        <div class="eyebrow">
            OPPORTUNITY DETAILS
        </div>

        <h2 id="jmDrawerTitle">
            ${escapeHtml(vacancy.title)}
        </h2>

        <div class="company-line">
            ${escapeHtml(company)}
        </div>

        <div class="location-line">
            ◉ ${escapeHtml(locationText)}
        </div>

        <div class="jm-detail-section">
            <h3>About this position</h3>

            <div class="jm-detail-description">
                ${escapeHtml(
        vacancy.description ||
        "No description provided."
    )}
            </div>
        </div>

        <div class="jm-detail-section">
            <h3>Required Skills</h3>

            <div class="skill-list">
                ${renderSkillChips(required)}
            </div>
        </div>

        <div class="jm-detail-section">
            <h3>Optional Skills</h3>

            <div class="skill-list">
                ${renderSkillChips(optional, true)}
            </div>
        </div>

        <div class="jm-detail-section">
            <h3>Salary</h3>
            <p class="muted">
                ${escapeHtml(salary)}
            </p>
        </div>

        <div class="jm-detail-actions">
            <button
                type="button"
                class="btn btn-primary"
                id="jmAnalyzeFromDrawer">
                Analyze Match ✦
            </button>

            ${applicationUrl
            ? `<a
                       class="btn btn-secondary"
                       href="${escapeHtml(applicationUrl)}"
                       target="_blank"
                       rel="noopener noreferrer">
                       Apply Now ↗
                   </a>`
            : `<button
                       type="button"
                       class="btn btn-secondary"
                       disabled
                       title="No valid application URL">
                       Apply Now ↗
                   </button>`
        }
        </div>

        <div class="jm-detail-note">
            Application availability is not verified.
        </div>
    `;

    content.querySelector("#jmAnalyzeFromDrawer")
        .addEventListener("click", () => {
            selectVacancyForAnalysis(vacancy.id);
        });

    root.classList.add("open");
    root.setAttribute("aria-hidden", "false");

    document.body.classList.add("jm-drawer-open");

    $("jmDrawerClose")?.focus();
}

function closeVacancyDrawer() {
    const root = $("vacancyDrawer");

    if (!root || !root.classList.contains("open")) {
        return;
    }

    root.classList.remove("open");
    root.setAttribute("aria-hidden", "true");

    document.body.classList.remove("jm-drawer-open");

    if (lastFocusedElement?.isConnected) {
        lastFocusedElement.focus();
    }
}

/* ==========================================
   LOAD VACANCIES
   ========================================== */

async function loadVacancies() {
    try {
        const vacancies = await apiGet("/api/Vacancies");

        if (!Array.isArray(vacancies)) {
            throw new Error("Invalid vacancies response");
        }

        allVacancies = vacancies;

        if ($("vacancyCount")) {
            $("vacancyCount").textContent =
                vacancies.length;
        }

        const select = $("vacancySelect");

        if (select) {
            const previous = select.value;

            select.replaceChildren();

            select.add(
                new Option("Select a vacancy", "")
            );

            vacancies.forEach(vacancy => {
                select.add(new Option(
                    vacancyLabel(vacancy),
                    String(vacancy.id)
                ));
            });

            if (vacancies.some(
                item => String(item.id) === previous
            )) {
                select.value = previous;
            }
        }

        searchVacancies();

        return true;
    } catch (error) {
        console.error("Vacancy loading failed:", error);

        if ($("vacancyCount")) {
            $("vacancyCount").textContent = "—";
        }

        if ($("vacancyResultCount")) {
            $("vacancyResultCount").textContent =
                "Unavailable";
        }

        if ($("vacancyList")) {
            $("vacancyList").innerHTML = `
                <div class="glass empty-state">
                    Unable to load vacancies.
                    Check your backend API.
                </div>
            `;
        }

        return false;
    }
}

/* ==========================================
   LOAD RESUMES
   ========================================== */

async function loadResumes() {
    try {
        const resumes = await apiGet("/api/Resumes");

        if (!Array.isArray(resumes)) {
            throw new Error("Invalid resumes response");
        }

        allResumes = resumes;

        if ($("resumeCount")) {
            $("resumeCount").textContent =
                resumes.length;
        }

        const selectors = [
            $("resumeSelect"),
            $("uploadResumeSelect")
        ].filter(Boolean);

        selectors.forEach(select => {
            const previous = select.value;

            select.replaceChildren();

            select.add(
                new Option("Select a resume", "")
            );

            resumes.forEach(resume => {
                select.add(new Option(
                    resume.userName ||
                    `Resume ${resume.id}`,
                    String(resume.id)
                ));
            });

            if (resumes.some(
                item => String(item.id) === previous
            )) {
                select.value = previous;
            }
        });

        return true;
    } catch (error) {
        console.error("Resume loading failed:", error);

        if ($("resumeCount")) {
            $("resumeCount").textContent = "—";
        }

        return false;
    }
}

/* ==========================================
   CV UPLOAD
   ========================================== */

async function uploadResume() {
    const resumeId =
        $("uploadResumeSelect")?.value;

    const file =
        $("cvFile")?.files?.[0];

    const status = $("uploadStatus");
    const button = $("uploadButton");

    if (!status || !button) return;

    status.classList.remove("error");

    if (!resumeId) {
        status.textContent = "Select a resume profile.";
        status.classList.add("error");
        return;
    }

    if (!file) {
        status.textContent = "Select a CV document.";
        status.classList.add("error");
        return;
    }

    const extension = file.name
        .split(".")
        .pop()
        .toLowerCase();

    if (!["pdf", "docx"].includes(extension)) {
        status.textContent =
            "Only PDF and DOCX files are supported.";

        status.classList.add("error");
        return;
    }

    if (file.size > 5 * 1024 * 1024) {
        status.textContent =
            "Maximum file size is 5 MB.";

        status.classList.add("error");
        return;
    }

    button.disabled = true;
    button.textContent = "Uploading...";

    status.textContent = "Uploading CV...";

    try {
        const formData = new FormData();

        formData.append("file", file);

        const response = await fetch(
            `/api/Resumes/${encodeURIComponent(resumeId)}/upload-cv`,
            {
                method: "POST",
                body: formData
            }
        );

        if (!response.ok) {
            throw new Error(`HTTP ${response.status}`);
        }

        status.textContent =
            "CV uploaded successfully.";

        if ($("resumeSelect")) {
            $("resumeSelect").value = resumeId;
        }

        $("cvFile").value = "";

        clearMatchResult();
    } catch (error) {
        console.error("CV upload error:", error);

        status.textContent =
            "CV upload failed. Check your backend.";

        status.classList.add("error");
    } finally {
        button.disabled = false;
        button.textContent = "Upload Resume ↗";
    }
}

/* ==========================================
   MATCH RESULT HELPERS
   ========================================== */

function resultGroup(title, items, extraClass = "") {
    const values = Array.isArray(items) ? items : [];

    const content = values.length
        ? `<ul>
            ${values.map(item => `
                <li>${escapeHtml(item)}</li>
            `).join("")}
           </ul>`
        : "<p>None</p>";

    return `
        <div class="result-group ${extraClass}">
            <h3>${escapeHtml(title)}</h3>
            ${content}
        </div>
    `;
}

function clearMatchResult() {
    analysisVersion++;

    if ($("matchResult")) {
        $("matchResult").replaceChildren();
    }
}

/* ==========================================
   ALWAYS VISIBLE APPLY NOW
   ========================================== */

function createApplyNowSection(vacancy) {
    const applicationUrl = getApplicationUrl(vacancy);

    const section = document.createElement("div");

    section.className = "jm-result-apply";

    const information = document.createElement("div");

    information.className = "jm-result-apply-info";

    const heading = document.createElement("strong");

    heading.textContent = "Ready for the next step?";

    const vacancyName = document.createElement("span");

    vacancyName.textContent =
        vacancy?.title || "Selected vacancy";

    information.append(heading, vacancyName);

    section.appendChild(information);

    if (applicationUrl) {
        const button = document.createElement("a");

        button.className =
            "btn btn-primary jm-result-apply-btn";

        button.href = applicationUrl;
        button.target = "_blank";
        button.rel = "noopener noreferrer";

        button.textContent = "Apply Now ↗";

        section.appendChild(button);
    } else {
        const button = document.createElement("button");

        button.type = "button";
        button.disabled = true;

        button.className =
            "btn btn-primary jm-result-apply-btn";

        button.textContent = "Apply Now ↗";

        button.title =
            "This vacancy has no valid application URL.";

        section.appendChild(button);
    }

    return section;
}

/* ==========================================
   RENDER MATCH RESULTS
   ========================================== */

function renderMatchResult(data, vacancy) {
    const result = $("matchResult");

    if (!result) return;

    const percentage = Number(data.matchPercentage);

    const percent = Math.max(
        0,
        Math.min(100, percentage)
    );

    const formatted = Number(percent.toFixed(1));

    result.innerHTML = `
        <div class="result-panel">

            <div class="eyebrow">
                YOUR SKILL MATCH SCORE
            </div>

            <div class="match-number">
                ${formatted}%
            </div>

            <div class="progress-track">
                <div
                    class="progress-fill"
                    id="progressFill">
                </div>
            </div>

            <div class="result-columns">

                ${resultGroup(
        "✓ Matched Skills",
        data.matchedSkills
    )}

                ${resultGroup(
        "Missing Required Skills",
        data.missingRequiredSkills
    )}

                ${resultGroup(
        "Missing Optional Skills",
        data.missingOptionalSkills
    )}

                ${resultGroup(
        "Recommendations",
        data.recommendations,
        "recommendation-group"
    )}

            </div>

        </div>
    `;

    const panel = result.querySelector(".result-panel");

    // Tugma har doim natija paneliga qo'shiladi
    panel.appendChild(
        createApplyNowSection(vacancy)
    );

    // Izoh
    const note = document.createElement("p");

    note.className = "jm-result-note";

    note.textContent =
        "Match score represents weighted skill overlap, " +
        "not your probability of being hired. " +
        "Application availability is not verified.";

    panel.appendChild(note);

    requestAnimationFrame(() => {
        const fill = $("progressFill");

        if (fill) {
            fill.style.width = percent + "%";
        }
    });
}

/* ==========================================
   MATCH ANALYSIS
   ========================================== */

async function analyzeMatch() {
    const resumeId = $("resumeSelect")?.value;
    const vacancyId = $("vacancySelect")?.value;

    const button = $("analyzeButton");
    const result = $("matchResult");

    if (!button || !result) return;

    if (!resumeId || !vacancyId) {
        result.textContent =
            "Please select a resume and a vacancy.";
        return;
    }

    const currentVersion = ++analysisVersion;

    button.disabled = true;
    button.textContent = "Analyzing...";

    result.textContent = "Analyzing your skills...";

    try {
        const url =
            `/api/JobMatch/compare` +
            `?resumeId=${encodeURIComponent(resumeId)}` +
            `&vacancyId=${encodeURIComponent(vacancyId)}`;

        const data = await apiGet(url);

        if (currentVersion !== analysisVersion) {
            return;
        }

        if (
            $("resumeSelect").value !== resumeId ||
            $("vacancySelect").value !== vacancyId
        ) {
            return;
        }

        const percentage = Number(data.matchPercentage);

        if (!Number.isFinite(percentage)) {
            throw new Error("Invalid match percentage");
        }

        const vacancy = allVacancies.find(
            item => String(item.id) === String(vacancyId)
        );

        // Vakansiya topilmasa ham tugma joyi chiqadi
        renderMatchResult(data, vacancy || {
            id: vacancyId,
            title: $("vacancySelect")
                .selectedOptions[0]?.textContent ||
                "Selected vacancy"
        });

    } catch (error) {
        console.error("Match analysis failed:", error);

        if (currentVersion === analysisVersion) {
            result.textContent =
                "Analysis failed. Check the API and try again.";
        }
    } finally {
        button.disabled = false;
        button.textContent = "Analyze My Match ✦";
    }
}

/* ==========================================
   EVENT LISTENERS
   ========================================== */

$("vacancySearch")?.addEventListener(
    "input",
    searchVacancies
);

$("uploadButton")?.addEventListener(
    "click",
    uploadResume
);

$("analyzeButton")?.addEventListener(
    "click",
    analyzeMatch
);

["resumeSelect", "vacancySelect"].forEach(id => {
    $(id)?.addEventListener(
        "change",
        clearMatchResult
    );
});

$("jmDrawerClose")?.addEventListener(
    "click",
    closeVacancyDrawer
);

$("jmDrawerBackdrop")?.addEventListener(
    "click",
    closeVacancyDrawer
);

document.addEventListener("keydown", event => {
    const root = $("vacancyDrawer");

    if (!root?.classList.contains("open")) {
        return;
    }

    if (event.key === "Escape") {
        closeVacancyDrawer();
        return;
    }

    if (event.key !== "Tab") return;

    const focusable = [
        ...root.querySelectorAll(
            'button:not(:disabled), a[href]'
        )
    ];

    if (!focusable.length) return;

    const first = focusable[0];
    const last = focusable[focusable.length - 1];

    if (
        event.shiftKey &&
        document.activeElement === first
    ) {
        event.preventDefault();
        last.focus();
    } else if (
        !event.shiftKey &&
        document.activeElement === last
    ) {
        event.preventDefault();
        first.focus();
    }
});

/* ==========================================
   START APPLICATION
   ========================================== */

async function startApp() {
    navigate();

    const results = await Promise.all([
        loadVacancies(),
        loadResumes()
    ]);

    setApiStatus(results.every(Boolean));

    console.info(
        "JobMatch frontend v3 loaded - Apply Now enabled"
    );
}

startApp();
