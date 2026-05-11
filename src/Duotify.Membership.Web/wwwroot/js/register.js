document.addEventListener("DOMContentLoaded", () => {
  const form = document.querySelector("form[action='/register']");
  if (!form) {
    return;
  }

  form.addEventListener("submit", () => {
    const button = form.querySelector("button[type='submit']");
    if (!(button instanceof HTMLButtonElement)) {
      return;
    }

    button.disabled = true;
    button.textContent = "送出中...";
  });
});