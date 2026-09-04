const express = require('express');
const mongoose = require('mongoose');
const cors = require('cors');
const nodemailer = require('nodemailer');
require('dotenv').config();

const app = express();

app.use(cors({
  origin: '*',
  methods: ['GET', 'POST', 'PUT', 'DELETE', 'OPTIONS'],
  allowedHeaders: ['Content-Type', 'Authorization']
}));

app.use(express.json());

const userSchema = new mongoose.Schema({
  username: { type: String, required: true },
  email: { type: String, required: true, unique: true },
  password: { type: String, required: true },
  resetOTP: String,
  otpExpires: Date
});
const User = mongoose.model('User', userSchema);

const eventSchema = new mongoose.Schema({
  eventTitle: { type: String, required: true },
  location: { type: String, required: true },
  date: { type: String, required: true },
  price: { type: Number, required: true },
  availableTickets: { type: Number, default: 100 }
});
const Event = mongoose.model('Event', eventSchema);

const ticketSchema = new mongoose.Schema({
  userId: { type: String, required: false },
  username: { type: String, required: false },
  email: { type: String, required: false },
  eventTitle: { type: String, required: false },
  location: { type: String, required: false },
  date: { type: String, required: false },
  price: { type: mongoose.Schema.Types.Mixed, required: false },
  ticketId: { type: String, required: false },
  ticketType: { type: String, required: false },
  quantity: { type: Number, required: false },
  totalPrice: { type: Number, required: false },
  paymentMethod: { type: String, required: false }
}, { strict: false });
const Ticket = mongoose.model('Ticket', ticketSchema);

const transporter = nodemailer.createTransport({
  service: 'gmail',
  auth: {
    user: process.env.EMAIL_USER,
    pass: process.env.EMAIL_PASS
  }
});

app.post('/api/register', async (req, res) => {
  try {
    const { name, email, password } = req.body;
    if (!name || !email || !password) {
      return res.status(400).json({ message: 'အချက်အလက်များကို အစအဆုံး ဖြည့်စွက်ပါ။' });
    }

    const existingUser = await User.findOne({ email });
    if (existingUser) {
      return res.status(400).json({ message: 'ဤ အီးမေးလ်ဖြင့် အကောင့်ရှိပြီးသား ဖြစ်ပါသည်။' });
    }

    const newUser = new User({ username: name, email, password });
    await newUser.save();
    
    res.status(201).json({ message: 'အကောင့်ဖွင့်ခြင်း အောင်မြင်ပါသည်။' });
  } catch (err) {
    res.status(500).json({ message: 'Server Error', error: err.message });
  }
});

app.post('/api/login', async (req, res) => {
  try {
    const { email, password } = req.body;
    if (!email || !password) {
      return res.status(400).json({ message: 'အီးမေးလ်နှင့် စကားဝှက်ကို ထည့်ပါ။' });
    }

    const user = await User.findOne({ email, password });
    if (!user) {
      return res.status(401).json({ message: 'အီးမေးလ် သို့မဟုတ် စကားဝှက် မှားယွင်းနေပါသည်။' });
    }

    res.json({ 
      message: 'Login အောင်မြင်ပါသည်။', 
      user: { id: user._id, username: user.username, email: user.email } 
    });
  } catch (err) {
    res.status(500).json({ message: 'Server Error', error: err.message });
  }
});

app.post('/api/forgot-password', async (req, res) => {
  try {
    const { email } = req.body;
    const user = await User.findOne({ email });
    if (!user) {
      return res.status(404).json({ message: 'ဤအီးမေးလ်ဖြင့် မှတ်ပုံတင်ထားခြင်း မရှိပါ။' });
    }
    const otp = Math.floor(100000 + Math.random() * 900000).toString();
    user.resetOTP = otp;
    user.otpExpires = Date.now() + 10 * 60 * 1000;
    await user.save();

    const mailOptions = {
      from: process.env.EMAIL_USER,
      to: email,
      subject: 'Password Reset OTP',
      text: `သင့်ရဲ့ Password ပြန်ပြောင်းရန် OTP ကုဒ်မှာ ${otp} ဖြစ်ပါသည်။ ဤကုဒ်သည် ၁၀ မိနစ်သာ ခံမည်ဖြစ်သည်။`
    };

    await transporter.sendMail(mailOptions);
    res.json({ message: 'OTP ကုဒ်ကို သင့်အီးမေးလ်သို့ ပို့ပြီးပါပြီ။' });
  } catch (err) {
    res.status(500).json({ message: 'Server Error', error: err.message });
  }
});

app.post('/api/reset-password', async (req, res) => {
  try {
    const { email, otp, otpCode, newPassword } = req.body;
    const codeToVerify = otp || otpCode; 

    const user = await User.findOne({ 
      email, 
      resetOTP: codeToVerify, 
      otpExpires: { $gt: Date.now() } 
    });

    if (!user) {
      return res.status(400).json({ message: 'OTP ကုဒ် မှားယွင်းနေသည် သို့မဟုတ် အချိန်ကုန်သွားပါပြီ။' });
    }

    user.password = newPassword;
    user.resetOTP = undefined;
    user.otpExpires = undefined;
    await user.save();

    res.json({ message: 'စကားဝှက် အသစ်လဲလှယ်ခြင်း အောင်မြင်ပါသည်။' });
  } catch (err) {
    res.status(500).json({ message: 'Server Error', error: err.message });
  }
});

app.get('/api/events', async (req, res) => {
  try {
    const events = await Event.find({});
    const formattedEvents = events.map(ev => ({
      id: ev._id,
      title: ev.eventTitle,
      venue: ev.location,
      eventDate: ev.date,
      ticketPrice: ev.price,
      availableTickets: ev.availableTickets
    }));
    res.json(formattedEvents);
  } catch (err) {
    res.status(500).json({ message: 'Error fetching events', error: err.message });
  }
});

app.post('/api/events', async (req, res) => {
  try {
    const { title, venue, eventDate, ticketPrice, availableTickets } = req.body;
    const newEvent = new Event({
      eventTitle: title,
      location: venue,
      date: eventDate,
      price: ticketPrice || 10000,
      availableTickets: availableTickets || 100
    });
    await newEvent.save();
    res.status(201).json({ message: 'Event created successfully', eventId: newEvent._id });
  } catch (err) {
    res.status(500).json({ message: 'Error creating event', error: err.message });
  }
});

app.put('/api/events/:id', async (req, res) => {
  try {
    const { id } = req.params;
    const { title, venue, eventDate, ticketPrice, availableTickets } = req.body;
    await Event.findByIdAndUpdate(id, {
      eventTitle: title,
      location: venue,
      date: eventDate,
      price: ticketPrice,
      availableTickets
    });
    res.json({ message: 'Event updated successfully' });
  } catch (err) {
    res.status(500).json({ message: 'Error updating event', error: err.message });
  }
});

app.delete('/api/events/:id', async (req, res) => {
  try {
    const { id } = req.params;
    await Event.findByIdAndDelete(id);
    res.json({ message: 'Event deleted successfully' });
  } catch (err) {
    res.status(500).json({ message: 'Error deleting event', error: err.message });
  }
});

app.post('/api/tickets', async (req, res) => {
  try {
    const { userId, username, email, eventTitle, location, date, price, ticketId, ticketType, quantity, totalPrice, paymentMethod } = req.body;
    const newTicket = new Ticket({
      userId, username, email, eventTitle, location, date, price, ticketId, ticketType, quantity, totalPrice, paymentMethod
    });
    await newTicket.save();
    res.status(201).json({ message: 'Ticket saved successfully!', ticket: req.body });
  } catch (err) {
    res.status(500).json({ message: 'Error saving ticket', error: err.message });
  }
});

app.get('/api/tickets/user/:userId', async (req, res) => {
  try {
    const userId = req.params.userId;
    const tickets = await Ticket.find({ userId });
    res.json(tickets || []);
  } catch (err) {
    res.status(500).json({ message: 'Error fetching user tickets', error: err.message });
  }
});

app.delete('/api/tickets/:id', async (req, res) => {
  try {
    const targetId = req.params.id;
    let deletedTicket = null;

    if (mongoose.Types.ObjectId.isValid(targetId)) {
      deletedTicket = await Ticket.findByIdAndDelete(targetId);
    }

    if (!deletedTicket) {
      deletedTicket = await Ticket.findOneAndDelete({ ticketId: targetId });
    }
    
    if (!deletedTicket) {
      return res.status(404).json({ message: 'ဖျက်မည့် လက်မှတ်ကို ရှာမတွေ့ပါ။' });
    }
    
    res.status(200).json({ message: 'လက်မှတ်ကို အောင်မြင်စွာ ဖျက်ပြီးပါပြီ။' });
  } catch (err) {
    res.status(500).json({ message: 'Error deleting ticket', error: err.message });
  }
});

app.post('/api/tickets/verify', async (req, res) => {
  try {
    const { ticketId } = req.body;
    if (!ticketId) {
      return res.status(400).json({ success: false, message: 'Ticket ID လိုအပ်ပါသည်။' });
    }

    const row = await Ticket.findOne({ ticketId });
    if (!row) {
      return res.status(404).json({ success: false, message: 'ဤ Ticket ID မှာ မှားယွင်းနေပါသည် သို့မဟုတ် မရှိပါ။' });
    }

    res.json({
      success: true,
      message: 'လက်မှတ်အချက်အလက် မှန်ကန်ပါသည်!',
      ticket: {
        ticketId: row.ticketId,
        eventTitle: row.eventTitle,
        userName: row.username,
        email: row.email,
        ticketType: row.ticketType,
        quantity: row.quantity
      }
    });
  } catch (err) {
    res.status(500).json({ success: false, message: 'Database error', error: err.message });
  }
});

const PORT = process.env.PORT || 8000;
const MONGO_URI = process.env.MONGO_URI;

mongoose.connect(MONGO_URI)
  .then(() => {
    console.log('🍃 MongoDB Connected Successfully!');
    app.listen(PORT, () => {
      console.log(`🚀 Server running on http://localhost:${PORT}`);
    });
  })
  .catch(err => {
    console.error('❌ MongoDB Connection Error:', err.message);
  });