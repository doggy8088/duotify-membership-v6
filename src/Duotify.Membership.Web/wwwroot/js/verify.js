document.addEventListener("DOMContentLoaded", () => {
  const input = document.querySelector(".verification-code-input");
  if (!(input instanceof HTMLInputElement)) {
    return;
  }

  input.addEventListener("input", () => {
    input.value = input.value.replace(/\D/g, "").slice(0, 6);
  });
});