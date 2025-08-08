import { create, style } from "../componentUtilities";

export var createPopup = (
  content: HTMLElement,
  onHide: () => void = () => {},
) => {
  var hide = (e: MouseEvent) => {
    if (e.target !== backgroundDiv) return;

    onHide();
    backgroundDiv.classList.remove("visible");
  };

  var backgroundDiv = create("div", {
    id: "backgroundDiv",
    className: "noTransition",
    onclick: hide,
  }, [
    create("div", { id: "contentDiv" }, [
      content,
    ]),
  ]);

  return {
    popup: style(
      "/website/components/popup.css",
      backgroundDiv,
    ),
    show: () => {
      backgroundDiv.classList.remove("noInitialTransition");
      backgroundDiv.classList.add("visible");
    },
    hide: hide,
  };
};
