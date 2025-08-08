import { send } from "../clientUtilities";
import { create, style } from "../componentUtilities";
import { createPopup } from "./popup";

export var createAddBookPopup = async (userId: string) => {
  var titleInput = create("input", { id: "titleInput" });
  var authorInput = create("input", { id: "authorInput" });
  var coverInput = create("input", { type: "file", id: "coverInput", accept: "image/*" });
  var coverImg = create("img", { id: "coverImg" });
  var descriptionTextArea = create("textarea", { id: "descInput" });
  var sumbitButton = create("button", { id: "submitButton" }, ["Submit"]);

  // add options to authorDiv
  // ========================
  var authorsList = create("datalist", { id: "authorsList" });
  var authorNames = await send<string[]>("getAuthorNames");
  var nameOptions = authorNames.map((name) => create("option", { value: name }));
  authorsList.append(...nameOptions);
  authorInput.setAttribute("list", authorsList.id);

  // load image using coverInput
  // ===========================
  coverInput.onchange = () => {
    var file = coverInput.files?.[0];
    if (file == null) {
      coverImg.src = "";
      return;
    }

    var reader = new FileReader();
    reader.onload = () => {
      coverImg.src = reader.result as string;
    };
    reader.readAsDataURL(file);
  };

  // add book on submitButton click
  // ==============================
  sumbitButton.onclick = async () => {
    var title = titleInput.value;
    var authorName = authorInput.value;
    var image = coverImg.src;
    var description = descriptionTextArea.value;
    await send("addBook", title, authorName, image, description, userId);
    location.reload();
  };

  return createPopup(
    style(
      "/website/components/addBookPopup.css",
      create("div", { id: "containerDiv" }, [
        create("h1", {}, ["Add Book"]),
        create("table", {}, [
          create("tr", {}, [
            create("td", {}, ["Title: "]),
            create("td", {}, [titleInput]),
          ]),
          create("tr", {}, [
            create("td", {}, ["Author: "]),
            create("td", {}, [authorInput]),
          ]),
          create("tr", {}, [
            create("td", {}, ["Cover Image: "]),
            create("td", { id: "imageTd" }, [
              coverInput,
              coverImg,
            ]),
          ]),
          create("tr", {}, [
            create("td", {}, ["Description: "]),
            create("td", {}, [descriptionTextArea]),
          ]),
        ]),
        sumbitButton,
        authorsList,
      ]),
    ),
    () => {
      titleInput.value = "";
      authorInput.value = "";
      coverInput.value = "";
      coverImg.src = "";
      descriptionTextArea.value = "";
    }
  );
};
