document.addEventListener('DOMContentLoaded', function () {
  var toggle = document.getElementById('mobileToggle');
  var sidebar = document.getElementById('sidebar');
  if (toggle && sidebar) {
    toggle.addEventListener('click', function () {
      sidebar.classList.toggle('open');
    });
  }
});
