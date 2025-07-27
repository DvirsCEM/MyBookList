/**
 * Creates an HTML element with specified properties and children.
 * @param tagName - The name of the HTML tag to create.
 * @param props - An object containing properties to set on the element. Defaults to null.
 * @param children - An array of child elements or strings to append to the created element. Defaults to null.
 * @returns The created HTML element with the specified properties and children.
 */
export function create<
  K extends keyof HTMLElementTagNameMap,
>(
  tagName: K,
  props: Partial<HTMLElementTagNameMap[K]> | null = null,
  children: (HTMLElement | string)[] | null = null,
): HTMLElementTagNameMap[K] {
  const element = document.createElement(tagName) as HTMLElementTagNameMap[K];

  if (props) {
    Object.assign(element, props);
  }

  if (children) {
    for (const child of children) {
      element.append(child);
    }
  }

  return element;
}

/**
 * Applies a CSS style to an HTML element by fetching the CSS from a given path.
 * @param cssPath - The path to the CSS file to be applied.
 * @param element - The HTML element to which the CSS will be applied.
 * @returns A wrapper element containing the styled element.
 */
export function style(cssPath: string, element: HTMLElement): HTMLElement {
  const wrapper = document.createElement("div");
  wrapper.style.display = "none";
  const shadow = wrapper.attachShadow({ mode: "open" });

  fetch(cssPath)
    .then((response) => response.text())
    .then((cssText) => {
      shadow.prepend(create("style", { textContent: cssText }));
      wrapper.style.display = "block";
    });
  shadow.appendChild(element);
  return wrapper;
}
