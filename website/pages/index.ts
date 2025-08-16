import { send } from "../clientUtilities";
import { createAddBookButton } from "../components/addBookButton";
import { createBanner } from "../components/banner";
import { createShelf } from "../components/shelf";
import { getUserId } from "../tools/funcs";
import { Book } from "../tools/types";

var userId = await getUserId();

var bannerDiv = document.querySelector<HTMLDivElement>("#bannerDiv")!;
var shelfDiv = document.querySelector<HTMLDivElement>("#shelfDiv")!;
var addBookButtonDiv = document.querySelector<HTMLDivElement>("#addBookButtonDiv")!;

bannerDiv.append(await createBanner(userId))

if (userId != null) {
  addBookButtonDiv.append(await createAddBookButton(userId));
}

var books = await send<Book[]>("getBooks");

shelfDiv.append(createShelf("All Books", books));