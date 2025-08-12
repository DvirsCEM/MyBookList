import { send } from "../clientUtilities";
import { create, style } from "../componentUtilities";
import { createPopup } from "./popup";

export var createLogInPopup = () => {
  var usernameInput = create("input", { type: "text", id: "usernameInput" });
  var passwordInput = create("input", { type: "password" });
  var alertDiv = create("div", { id: "alertDiv" }, []);
  var submitButton = create("button", { id: "submitButton" }, ["Submit"]);

  submitButton.onclick = async () => {
    var userId = await send<string | null>(
      "logIn",
      usernameInput.value,
      passwordInput.value,
    );

    if (userId == null) {
      alertDiv.innerText = "Wrong username or password.";
      return;
    }

    localStorage.setItem("userId", userId);
    window.location.reload();
  };

  return createPopup(
    style(
      "/website/components/logInPopup.css",
      create("div", { id: "containerDiv" }, [
        create("h1", {}, ["Log In"]),
        create("table", { id: "formTable" }, [
          create("tr", {}, [
            create("td", {}, ["Username: "]),
            create("td", {}, [usernameInput]),
          ]),
          create("tr", {}, [
            create("td", {}, ["Password: "]),
            create("td", {}, [passwordInput]),
          ]),
        ]),
        alertDiv,
        submitButton,
      ]),
    ),
    () => {
      usernameInput.innerText = "";
      passwordInput.innerText = "";
      alertDiv.innerText = "";
    },
  );
};
