import { send } from "../clientUtilities";
import { create, style } from "../componentUtilities";
import { createLogInPopup } from "./logInPopup";
import { createSignUpPopup } from "./signUpPopup";

export var createBanner = async (userId: string | null) => {
  var banner = create("div", { id: "banner" }, [
    create("a", { id: "indexAnchor", href: "index.html" }, [
      create("h1", { id: "titleH1" }, ["MyBookList"]),
    ]),
    create("div", { id: "userDiv" }),
  ]);

  var userDiv = banner.querySelector("#userDiv")!;

  if (userId != null) {
    var username = await send("getUsername", userId);

    userDiv.append(
      create("div", {}, [`Welcome, ${username}!`]),
      create("a", { href: "profile.html?user=${username}" }, ["My Profile"]),
      create("button", { onclick: logOut }, ["Log Out"]),
    );
  } else {
    var { popup: logInPopup, show: showLogInPopup } = createLogInPopup();
    var { popup: signUpPopup, show: showSignUpPopup } = createSignUpPopup();

    document.body.append(
      logInPopup,
      signUpPopup,
    );
    userDiv.append(
      create("button", { onclick: showLogInPopup }, ["Log In"]),
      create("button", { onclick: showSignUpPopup }, ["Sign Up"]),
    );
  }

  return style("/website/components/banner.css", banner);
};

var logOut = () => {
  localStorage.removeItem("userSecret");
  location.href = "/website/pages/index.html";
};
