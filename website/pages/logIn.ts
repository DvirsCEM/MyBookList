import { send } from "clientUtilities";
import { get } from "componentUtilities";
import { createBar } from "scripts/funcs";
import { User } from "scripts/types";

var usernameInput = get("input", "usernameInput");
var passwordInput = get("input", "passwordInput");
var submitButton = get("button", "submitButton");
var errorDiv = get("div", "errorDiv");

var token = localStorage.getItem("token");
var user = await send<User | null>("getUser", token);

document.body.prepend(createBar(user));

submitButton.onclick = async function () {
  var token = await send<string | null>("logIn", usernameInput.value, passwordInput.value);

  if (token == null) {
    errorDiv.innerText = "Invalid username or password.";
    return;
  }

  localStorage.setItem("token", token);
  location.href = "index.html";
};
