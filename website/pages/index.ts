import { send } from "../clientUtilities";
import { createAddBookPopup } from "../components/addBookPopup";
import { createBanner } from "../components/banner";
import { create } from "../componentUtilities";
import { getUserSecret } from "../tools/funcs";

var userId = await getUserSecret();

document.querySelector<HTMLDivElement>("#bannerDiv")!.append(
  await createBanner(userId),
);

var contentDiv = document.querySelector<HTMLDivElement>("#contentDiv")!;

if (userId != null) {
  var { popup, show } = await createAddBookPopup(userId);

  document.body.appendChild(popup);

  var addDiv = create("button", { id: "addButton", onclick: show }, [
    create("div", { id: "addDiv" }, ["+"]),
  ]);

  contentDiv.append(addDiv);
}

var books = await send("getBooks");