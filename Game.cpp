//
// Created by kellm on 2020-09-22.
//

#include "Game.h"

void Game::render() {
    window->clear();
    backgroundTexture.loadFromImage(image);
    background.setTexture(backgroundTexture);
    window->draw(background);
    window->display();
}

void Game::update() {
    while (window->pollEvent(ev))
    {
        switch (ev.type) {
            case sf::Event::Closed:
                window->close();
                break;
        }
    }
    if (sf::Mouse::isButtonPressed(sf::Mouse::Button::Left)){
        auto pos = sf::Mouse::getPosition(*window);
        sand->addElement(Materials::sand, pos.x, pos.y);
    }
    sand->update();
    image.create(videoMode.width, videoMode.height, sand->getPixelData());
}

Game::Game() {
    init_window();
}

void Game::init_window() {
    window = new sf::RenderWindow(videoMode,
                                  "Window", sf::Style::Titlebar | sf::Style::Close);
    image.create(videoMode.width, videoMode.height, sf::Color::Black);
    sand = new FallingSand(videoMode.width, videoMode.height);
}

Game::~Game() {
    delete window;
}

bool Game::running() const{
    return window->isOpen();
}

void Game::update_image(const uint8_t* ptr) {
    image.create(videoMode.width, videoMode.height, ptr);
}

sf::VideoMode Game::getVideoMode() const {
    return videoMode;
}
