import { send } from "../clientUtilities";
import { create, style } from "../componentUtilities";
import { createPopup } from "./popup";

export var createSignUpPopup = () => {
  var usernameInput = create("input", { type: "text", id: "usernameInput" });
  var passwordInput = create("input", { type: "password" });
  var confirmInput = create("input", { type: "password" });
  var alertDiv = create("div", { id: "alertDiv" }, []);
  var submitButton = create("button", { id: "submitButton" }, ["Submit"]);

  submitButton.onclick = async () => {
    if (passwordInput.value != confirmInput.value) {
      alertDiv.innerText = "Passwords do not match.";
      return;
    }

    var userId = await send<string | null>(
      "signUp",
      usernameInput.value,
      passwordInput.value,
    );

    if (userId == null) {
      alertDiv.innerText = "Username is already in use.";
      return;
    }

    localStorage.setItem("userId", userId);
    window.location.reload();
  };

  return createPopup(
    style(
      "/website/components/signUpPopup.css",
      create("div", { id: "containerDiv" }, [
        create("h1", {}, ["Sign Up"]),
        create("table", { id: "formTable" }, [
          create("tr", {}, [
            create("td", {}, ["Username: "]),
            create("td", {}, [usernameInput]),
          ]),
          create("tr", {}, [
            create("td", {}, ["Password: "]),
            create("td", {}, [passwordInput]),
          ]),
          create("tr", {}, [
            create("td", {}, ["Confirm Password: "]),
            create("td", {}, [confirmInput]),
          ]),
        ]),
        alertDiv,
        submitButton,
      ]),
    ),
    () => {
      usernameInput.innerText = "";
      passwordInput.innerText = "";
      confirmInput.innerText = "";
      alertDiv.innerText = "";
    },
  );
};
