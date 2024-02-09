import asyncio
import aiohttp
import random
import string
from tkinter import Tk, Label, Listbox, Scrollbar, Button, Frame
from threading import Thread

def generate_random_number(length):
    return ''.join(random.choices(string.digits, k=length))

async def fetch(session, url):
    async with session.get(url) as response:
        return response.status, await response.text(), url.split('/')[-1]

async def request_with_random_number(root, listbox, stop_event):
    async with aiohttp.ClientSession() as session:
        while not stop_event.is_set():
            random_number_length = random.randint(5, 7)
            random_number = generate_random_number(random_number_length)
            url = f"https://kahoot.it/reserve/session/{random_number}"
            status, text, code = await fetch(session, url)
            print(f"Code: {random_number}, Response: {status}")
            if status == 200:
                listbox.insert('end', code)
                # Use root.after to schedule GUI updates from the main thread
                root.after(0, listbox.update_idletasks)

def start_async_request(loop, root, listbox, stop_event):
    asyncio.set_event_loop(loop)
    loop.run_until_complete(request_with_random_number(root, listbox, stop_event))

def setup_gui():
    root = Tk()
    root.title("Kahoot Bot")
    frame = Frame(root)
    frame.pack(fill='both', expand=True)
    
    listbox = Listbox(frame, height=20, width=50)
    listbox.pack(side='left', fill='y')
    
    scrollbar = Scrollbar(frame, orient='vertical')
    scrollbar.config(command=listbox.yview)
    scrollbar.pack(side='right', fill='y')
    
    listbox.config(yscrollcommand=scrollbar.set)

    stop_event = asyncio.Event()

    def on_start():
        loop = asyncio.new_event_loop()
        t = Thread(target=start_async_request, args=(loop, root, listbox, stop_event))
        t.start()
    
    start_button = Button(root, text="Start Searching", command=on_start)
    start_button.pack(side='bottom')

    root.protocol("WM_DELETE_WINDOW", lambda: stop_event.set() or root.destroy())
    
    root.mainloop()

if __name__ == "__main__":
    setup_gui()
