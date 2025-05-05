import cv2
import mediapipe as mp
import socket
import time
import select
import threading


mp_hands = mp.solutions.hands
mp_drawing = mp.solutions.drawing_utils


cap = cv2.VideoCapture(0)


server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
server_ip = '127.0.0.1'
server_port = 12345
server_socket.bind((server_ip, server_port))
server_socket.listen(5)
print(f"Server listening on {server_ip}:{server_port}")

client_socket, client_address = server_socket.accept()
print(f"Connection established with {client_address}")
client_socket.setblocking(False)


def receive_data(sock):
    while True:
        try:
            ready_to_read, _, _ = select.select([sock], [], [], 0.01)
            if ready_to_read:
                data = sock.recv(1024)
                if data:
                    received = data.decode().strip()
                    print(f"Received from Unity: {received}")
        except Exception as e:
            print(f"Receive thread error: {e}")
            break

recv_thread = threading.Thread(target=receive_data, args=(client_socket,))
recv_thread.daemon = True
recv_thread.start()

with mp_hands.Hands(min_detection_confidence=0.5, min_tracking_confidence=0.5) as hands:
    last_sent_time = time.time()
    send_interval = 0.1  

    while cap.isOpened():
        ret, frame = cap.read()
        if not ret:
            print("Failed to read frame from camera.")
            break

        
        frame = cv2.flip(frame, 1)
        small = cv2.resize(frame, (frame.shape[1] // 2, frame.shape[0] // 2))
        rgb = cv2.cvtColor(small, cv2.COLOR_BGR2RGB)
        results = hands.process(rgb)

        
        left_hand_gesture = None
        right_hand_option = None
        spell = None
        left_present = False

        
        if results.multi_hand_landmarks:
            for lm, info in zip(results.multi_hand_landmarks, results.multi_handedness):
                label = info.classification[0].label  
                
                tips = {
                    'thumb': lm.landmark[mp_hands.HandLandmark.THUMB_TIP],
                    'index': lm.landmark[mp_hands.HandLandmark.INDEX_FINGER_TIP],
                    'middle': lm.landmark[mp_hands.HandLandmark.MIDDLE_FINGER_TIP],
                    'ring': lm.landmark[mp_hands.HandLandmark.RING_FINGER_TIP],
                    'pinky': lm.landmark[mp_hands.HandLandmark.PINKY_TIP]
                }
                pips = {
                    'index': lm.landmark[mp_hands.HandLandmark.INDEX_FINGER_PIP],
                    'middle': lm.landmark[mp_hands.HandLandmark.MIDDLE_FINGER_PIP],
                    'ring': lm.landmark[mp_hands.HandLandmark.RING_FINGER_PIP],
                    'pinky': lm.landmark[mp_hands.HandLandmark.PINKY_PIP]
                }

                
                if label == 'Left':
                    left_present = True
                    
                    if all(tips[f].y > pips[f].y for f in ['index', 'middle', 'ring', 'pinky']):
                        left_hand_gesture = 'fist'
                    
                    elif (tips['index'].y < tips['thumb'].y and tips['middle'].y < tips['thumb'].y and
                          abs(tips['index'].x - tips['middle'].x) < 0.15):
                        left_hand_gesture = 'victory'
                    
                    elif (all(tips[f].y < pips[f].y for f in ['index', 'middle', 'ring', 'pinky']) and
                          max(tips[f].x for f in ['index','middle','ring','pinky']) - min(tips[f].x for f in ['index','middle','ring','pinky']) < 0.1):
                        left_hand_gesture = 'high_five'

                
                if label == 'Right' and left_hand_gesture:
                    
                    idx = tips['index'].y < pips['index'].y
                    mid = tips['middle'].y < pips['middle'].y
                    rng = tips['ring'].y < pips['ring'].y
                    pnk = tips['pinky'].y < pips['pinky'].y

                    
                    if left_hand_gesture == 'fist':
                        if idx and not mid and not rng and not pnk:
                            right_hand_option = '1'; spell = 'MagicAttack'
                        elif idx and mid and not rng and not pnk:
                            right_hand_option = '2'; spell = 'FreezeAttack'
                        elif idx and mid and rng and not pnk:
                            right_hand_option = '3'; spell = 'PoisonAttack'
                        elif idx and mid and rng and pnk:
                            right_hand_option = '4'; spell = 'FireAttack'
                    elif left_hand_gesture == 'victory':
                        if rng and idx and mid and not pnk:
                            right_hand_option = '3'; spell = 'Cleanse'
                        elif not rng and idx and abs(tips['middle'].y - tips['index'].y) > 0.1:
                            right_hand_option = '1'; spell = 'InstantHeal'
                        elif not rng and idx and abs(tips['middle'].x - tips['index'].x) < 0.1:
                            right_hand_option = '2'; spell = 'Regeneration'
                    elif left_hand_gesture == 'high_five':
                        if idx and not mid and not rng and not pnk:
                            right_hand_option = '1'; spell = 'Shield'
                        elif idx and mid and not rng and not pnk:
                            right_hand_option = '2'; spell = 'Bubble'

        
        left_val = left_hand_gesture if left_present else 'none'
        right_val = right_hand_option if right_hand_option else 'none'
        spell_val = spell if spell else 'none'

        
        if (time.time() - last_sent_time) > send_interval:
            msg = f"left_hand:{left_val};right_hand:{right_val};spell:{spell_val}"
            try:
                client_socket.send(msg.encode())
                print(f"Sent: {msg}")
            except BrokenPipeError:
                print("Unity disconnected.")
                break
            last_sent_time = time.time()

        
        cv2.imshow("Hand Gesture Recognition", small)
        if cv2.waitKey(1) & 0xFF == ord('q'):
            break


cap.release()
cv2.destroyAllWindows()
client_socket.close()
server_socket.close()
print("Closed all connections.")
