(function () {
  const PAGES = [
    ["claims-list.html", "Claims"],
    ["claim-detail.html", "Claim detail"],
    ["fnol-intake.html", "New claim"]
  ];
  const ICON =
    '<svg class="sun" viewBox="0 0 24 24" aria-hidden="true"><circle cx="12" cy="12" r="4"/>' +
    '<path d="M12 2v2M12 20v2M2 12h2M20 12h2M4.9 4.9l1.4 1.4M17.7 17.7l1.4 1.4M4.9 19.1l1.4-1.4M17.7 6.3l1.4-1.4"/></svg>' +
    '<svg class="moon" viewBox="0 0 24 24" aria-hidden="true"><path d="M21 12.8A9 9 0 1 1 11.2 3a7 7 0 0 0 9.8 9.8z"/></svg>';
  const USERS = { Handler: "Hana Handler", Supervisor: "Sam Supervisor", Manager: "Mia Manager" };

  function read(key, fallback) {
    try { return localStorage.getItem(key) || fallback; } catch (e) { return fallback; }
  }
  function write(key, value) {
    try { localStorage.setItem(key, value); } catch (e) { /* storage unavailable */ }
  }

  const page = location.pathname.split("/").pop();
  const links = PAGES.map(function (p) {
    return '<a href="' + p[0] + '"' + (p[0] === page ? ' class="current"' : "") + ">" + p[1] + "</a>";
  }).join("");
  const options = Object.keys(USERS).map(function (r) { return "<option>" + r + "</option>"; }).join("");

  const bar = document.createElement("header");
  bar.className = "bar";
  bar.innerHTML =
    '<div class="left"><span class="brand">Claims Management</span><nav>' + links + "</nav></div>" +
    '<div class="user">' +
    '<button id="theme" class="icon-toggle" type="button" aria-label="Switch between dark and light theme" title="Switch theme">' + ICON + "</button>" +
    '<span id="who"></span><select id="role" aria-label="Role switcher (testing only)">' + options + "</select></div>";
  document.body.prepend(bar);

  function applyTheme(theme) {
    document.documentElement.dataset.theme = theme;
    write("theme", theme);
  }
  function applyRole(role) {
    document.getElementById("who").textContent = USERS[role] + " - " + role;
    document.getElementById("role").value = role;
    write("role", role);
    document.dispatchEvent(new CustomEvent("rolechange", { detail: role }));
  }

  applyTheme(read("theme", "dark"));
  document.getElementById("theme").onclick = function () {
    applyTheme(document.documentElement.dataset.theme === "dark" ? "light" : "dark");
  };
  document.getElementById("role").onchange = function (e) { applyRole(e.target.value); };
  window.Shell = { role: function () { return read("role", "Handler"); } };
  applyRole(read("role", "Handler"));
})();
