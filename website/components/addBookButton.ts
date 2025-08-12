import { create, style } from "../componentUtilities";
import { createAddBookPopup } from "./addBookPopup";

export var createAddBookButton = async (userId: string) => {
  var { popup, show } = await createAddBookPopup(userId);

  document.body.appendChild(popup);

  return style("/website/components/addBookButton.css",
    create("button", { id: "addButton", onclick: show }, [
      create("div", { id: "addDiv" }, ["+"]),
    ]));
}